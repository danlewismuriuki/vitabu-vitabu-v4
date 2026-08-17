"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { AuthShell, FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, AuthResponse, fieldErrors } from "@/lib/api";
import { saveSession } from "@/lib/auth-storage";

export default function SignupPage() {
  const router = useRouter();
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      const data = await apiFetch<AuthResponse>("/auth/register", {
        method: "POST",
        body: JSON.stringify({
          display_name: form.get("display_name"),
          email: form.get("email"),
          password: form.get("password"),
          city: form.get("city"),
          accept_terms: form.get("accept_terms") === "on",
          confirm_parent_guardian: form.get("confirm_parent_guardian") === "on",
        }),
      });
      saveSession(data.access_token, {
        id: data.user.id,
        display_name: data.user.display_name,
        email: data.user.email,
        city: data.user.city,
        phone_verified: data.user.phone_verified,
        phone_e164: data.user.phone_e164,
      });
      router.push("/verify-phone");
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Cannot reach the API. Is it running on :5080?"]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthShell
      title="Create your account"
      subtitle="For parents and guardians circulating CBC books across Kenya."
    >
      <form onSubmit={onSubmit} className="space-y-4">
        <FormError messages={errors} />
        <div>
          <label className={labelClass} htmlFor="display_name">
            Display name
          </label>
          <input id="display_name" name="display_name" required className={fieldClass} />
        </div>
        <div>
          <label className={labelClass} htmlFor="email">
            Email
          </label>
          <input id="email" name="email" type="email" required className={fieldClass} />
        </div>
        <div>
          <label className={labelClass} htmlFor="password">
            Password
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
        <div>
          <label className={labelClass} htmlFor="city">
            City
          </label>
          <input
            id="city"
            name="city"
            required
            placeholder="Nairobi"
            className={fieldClass}
          />
        </div>
        <label className="flex items-start gap-2 text-sm text-neutral-700">
          <input type="checkbox" name="confirm_parent_guardian" className="mt-1" required />
          I confirm I am 18+ / a parent or guardian.
        </label>
        <label className="flex items-start gap-2 text-sm text-neutral-700">
          <input type="checkbox" name="accept_terms" className="mt-1" required />
          I agree to the Terms and Privacy policy.
        </label>
        <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
          {loading ? "Creating account…" : "Sign up"}
        </button>
      </form>
      <p className="mt-4 text-sm text-neutral-600">
        Already have an account?{" "}
        <Link href="/login" className="text-accent-600 hover:text-accent-700">
          Log in
        </Link>
      </p>
    </AuthShell>
  );
}
