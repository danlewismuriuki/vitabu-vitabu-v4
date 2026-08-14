"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useState } from "react";
import { AuthShell, FormError, fieldClass, labelClass } from "@/components/AuthShell";
import { ApiError, apiFetch, AuthResponse, fieldErrors } from "@/lib/api";
import { saveSession } from "@/lib/auth-storage";

function LoginForm() {
  const router = useRouter();
  const search = useSearchParams();
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors([]);
    setLoading(true);
    const form = new FormData(e.currentTarget);
    try {
      const data = await apiFetch<AuthResponse>("/auth/login", {
        method: "POST",
        body: JSON.stringify({
          email: form.get("email"),
          password: form.get("password"),
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
      const returnUrl = search.get("returnUrl") || "/";
      router.push(returnUrl);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Something went wrong. Try again."]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthShell title="Log in" subtitle="Welcome back — continue circulating books.">
      <form onSubmit={onSubmit} className="space-y-4">
        <FormError messages={errors} />
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
          <input id="password" name="password" type="password" required className={fieldClass} />
        </div>
        <button type="submit" disabled={loading} className="btn-primary w-full disabled:opacity-60">
          {loading ? "Signing in…" : "Log in"}
        </button>
      </form>
      <p className="mt-4 text-sm text-neutral-600">
        <Link href="/forgot-password" className="text-accent-600 hover:text-accent-700">
          Forgot password?
        </Link>
      </p>
      <p className="mt-2 text-sm text-neutral-600">
        New here?{" "}
        <Link href="/signup" className="text-accent-600 hover:text-accent-700">
          Create an account
        </Link>
      </p>
    </AuthShell>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<AuthShell title="Log in">Loading…</AuthShell>}>
      <LoginForm />
    </Suspense>
  );
}
