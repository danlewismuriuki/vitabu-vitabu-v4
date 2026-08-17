"use client";

import Link from "next/link";
import { FormEvent, Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { AuthShell, FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors, UserProfile } from "@/lib/api";
import { getToken, getStoredUser, saveSession } from "@/lib/auth-storage";

function VerifyPhoneForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = searchParams.get("returnUrl") || "/";
  const [errors, setErrors] = useState<string[]>([]);
  const [info, setInfo] = useState<string | null>(null);
  const [devCode, setDevCode] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState<"phone" | "code">("phone");

  useEffect(() => {
    if (!getToken()) {
      router.replace(
        `/login?returnUrl=${encodeURIComponent(`/verify-phone?returnUrl=${returnUrl}`)}`
      );
    }
  }, [router, returnUrl]);

  async function requestOtp(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setInfo(null);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    const token = getToken();
    try {
      const res = await apiFetch<{
        message: string;
        expires_in_seconds: number;
        dev_code?: string | null;
      }>("/auth/phone/request-otp", {
        method: "POST",
        token,
        body: JSON.stringify({ phone_e164: form.get("phone_e164") }),
      });
      setInfo(res.message);
      setDevCode(res.dev_code ?? null);
      setStep("code");
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Could not send code."]);
    } finally {
      setLoading(false);
    }
  }

  async function verifyOtp(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    const token = getToken();
    try {
      const user = await apiFetch<UserProfile>("/auth/phone/verify-otp", {
        method: "POST",
        token,
        body: JSON.stringify({ code: form.get("code") }),
      });
      const stored = getStoredUser();
      if (token && stored) {
        saveSession(token, {
          ...stored,
          phone_verified: user.phone_verified,
          phone_e164: user.phone_e164,
        });
      }
      router.push(returnUrl.startsWith("/") ? returnUrl : "/");
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Could not verify code."]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthShell
      title="Verify your phone"
      subtitle="Required before you sell, message, or accept a deal. Browse stays open without it."
    >
      <FormError messages={errors} />
      {info ? <p className="mb-4 text-sm text-secondary-600">{info}</p> : null}
      {devCode ? (
        <p className="mb-4 rounded-lg bg-secondary-50 px-3 py-2 text-sm text-secondary-700">
          Dev OTP: <strong>{devCode}</strong>
        </p>
      ) : null}

      {step === "phone" ? (
        <form onSubmit={requestOtp} className="space-y-4">
          <div>
            <label className={labelClass} htmlFor="phone_e164">
              Kenyan mobile (E.164)
            </label>
            <input
              id="phone_e164"
              name="phone_e164"
              required
              placeholder="+254712345678"
              pattern="^\+2547\d{8}$"
              className={fieldClass}
            />
          </div>
          <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
            {loading ? "Sending…" : "Send SMS code"}
          </button>
        </form>
      ) : (
        <form onSubmit={verifyOtp} className="space-y-4">
          <div>
            <label className={labelClass} htmlFor="code">
              6-digit code
            </label>
            <input
              id="code"
              name="code"
              required
              inputMode="numeric"
              pattern="\d{6}"
              maxLength={6}
              className={fieldClass}
            />
          </div>
          <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
            {loading ? "Verifying…" : "Verify phone"}
          </button>
          <button
            type="button"
            className="btn-secondary w-full"
            onClick={() => setStep("phone")}
          >
            Change number
          </button>
        </form>
      )}

      <p className="mt-4 text-sm text-neutral-600">
        <Link href="/" className="text-accent-600 hover:text-accent-700">
          Skip for now — browse books
        </Link>
      </p>
    </AuthShell>
  );
}

export default function VerifyPhonePage() {
  return (
    <Suspense fallback={<AuthShell title="Verify your phone">Loading…</AuthShell>}>
      <VerifyPhoneForm />
    </Suspense>
  );
}
