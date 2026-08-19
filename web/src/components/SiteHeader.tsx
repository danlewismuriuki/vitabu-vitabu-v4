"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { getStoredUser, clearSession } from "@/lib/auth-storage";

export function SiteHeader({
  active,
}: {
  active?: "browse" | "sell" | "mine" | "wishlist";
}) {
  const [user, setUser] = useState<{ display_name: string; phone_verified: boolean } | null>(
    null
  );

  useEffect(() => {
    setUser(getStoredUser());
  }, []);

  function logout() {
    clearSession();
    setUser(null);
    window.location.href = "/";
  }

  const link = (href: string, key: typeof active, label: string) => (
    <Link
      href={href}
      className={
        active === key
          ? "font-medium text-accent-600"
          : "text-primary-700 hover:text-accent-600"
      }
    >
      {label}
    </Link>
  );

  return (
    <header className="border-b border-neutral-200 bg-white">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-5">
        <Link href="/" className="font-poppins text-xl font-bold text-primary-700">
          Vitabu Vitabu
        </Link>
        <nav className="flex flex-wrap items-center gap-4 text-sm">
          {link("/books", "browse", "Browse")}
          {link("/sell", "sell", "Sell")}
          {user ? link("/my-listings", "mine", "My listings") : null}
          {user ? link("/wishlist", "wishlist", "Wishlist") : null}
          {user ? (
            <Link href="/my-interests" className="text-primary-700 hover:text-accent-600">
              Interests
            </Link>
          ) : null}
          {user ? (
            <Link href="/notifications" className="text-primary-700 hover:text-accent-600">
              Alerts
            </Link>
          ) : null}
          {user ? (
            <>
              <span className="text-neutral-500">{user.display_name}</span>
              {!user.phone_verified ? (
                <Link href="/verify-phone" className="text-accent-600">
                  Verify phone
                </Link>
              ) : null}
              <button
                type="button"
                onClick={logout}
                className="text-neutral-500 hover:text-primary-700"
              >
                Log out
              </button>
            </>
          ) : (
            <Link href="/login" className="btn-secondary !px-4 !py-2 text-sm">
              Log in
            </Link>
          )}
        </nav>
      </div>
    </header>
  );
}
