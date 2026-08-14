"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useState } from "react";
import { AuthShell, FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";

function ResetForm() {
  const router = useRouter();
  const search = useSearchParams();
  const token = search.get("token") ?? "";
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      await apiFetch("/auth/reset-password", {
        method: "POST",
        body: JSON.stringify({
          token,
          password: form.get("password"),
        }),
      });
      router.push("/login");
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Something went wrong."]);
    } finally {
      setLoading(false);
    }
  }

  if (!token) {
    return (
      <AuthShell title="Reset password">
        <p className="text-sm text-accent-700">
          Missing reset token.{" "}
          <Link href="/forgot-password" className="underline">
            Request a new link
          </Link>
          .
        </p>
      </AuthShell>
    );
  }

  return (
    <AuthShell title="Choose a new password">
      <form onSubmit={onSubmit} className="space-y-4">
        <FormError messages={errors} />
        <div>
          <label className={labelClass} htmlFor="password">
            New password
          </label>
          <input
            id="password"
            name="password"
            type="password"
            required
            minLength={8}
            className={fieldClass}
          />
        </div>
        <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
          {loading ? "Saving…" : "Update password"}
        </button>
      </form>
    </AuthShell>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<AuthShell title="Reset password">Loading…</AuthShell>}>
      <ResetForm />
    </Suspense>
  );
}
