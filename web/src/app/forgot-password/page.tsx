"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { AuthShell, FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";

export default function ForgotPasswordPage() {
  const [errors, setErrors] = useState<string[]>([]);
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      await apiFetch("/auth/forgot-password", {
        method: "POST",
        body: JSON.stringify({ email: form.get("email") }),
      });
      setDone(true);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Something went wrong."]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthShell
      title="Forgot password"
      subtitle="We'll email a reset link if that account exists (check Mailpit locally)."
    >
      {done ? (
        <p className="text-sm text-secondary-600">
          If that email exists, we sent reset instructions.{" "}
          <Link href="/login" className="text-accent-600">
            Back to login
          </Link>
        </p>
      ) : (
        <form onSubmit={onSubmit} className="space-y-4">
          <FormError messages={errors} />
          <div>
            <label className={labelClass} htmlFor="email">
              Email
            </label>
            <input id="email" name="email" type="email" required className={fieldClass} />
          </div>
          <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
            {loading ? "Sending…" : "Send reset link"}
          </button>
        </form>
      )}
    </AuthShell>
  );
}
