"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ApiError, apiFetch, UserProfile } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type ListingCard = {
  id: string;
  title: string;
  grade: string;
  subject: string;
  city: string;
  intent: string;
  condition: string;
  status: string;
  price_kes?: number | null;
};

type WishlistItem = {
  listing_id: string;
  saved_at_utc: string;
  listing: ListingCard;
};

type WishlistPage = { items: WishlistItem[] };

export default function WishlistPage() {
  const router = useRouter();
  const [items, setItems] = useState<WishlistItem[]>([]);
  const [alertsEnabled, setAlertsEnabled] = useState(true);
  const [prefsBusy, setPrefsBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  async function load(token: string) {
    const [page, me] = await Promise.all([
      apiFetch<WishlistPage>("/me/wishlist?page_size=50", { token }),
      apiFetch<UserProfile>("/auth/me", { token }),
    ]);
    setItems(page.items);
    setAlertsEnabled(me.wishlist_alerts_enabled);
  }

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login?returnUrl=/wishlist");
      return;
    }
    (async () => {
      try {
        await load(token);
      } catch (err) {
        if (err instanceof ApiError) setError(err.problem.message);
        else setError("Could not load wishlist.");
      }
    })();
  }, [router]);

  async function toggleAlerts(next: boolean) {
    const token = getToken();
    if (!token) return;
    setPrefsBusy(true);
    setError(null);
    try {
      const me = await apiFetch<UserProfile>("/auth/me/notification-prefs", {
        method: "PATCH",
        token,
        body: JSON.stringify({ wishlist_alerts_enabled: next }),
      });
      setAlertsEnabled(me.wishlist_alerts_enabled);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not update alert preferences.");
    } finally {
      setPrefsBusy(false);
    }
  }

  async function remove(listingId: string) {
    const token = getToken();
    if (!token) return;
    setBusyId(listingId);
    try {
      await apiFetch(`/listings/${listingId}/wishlist`, { method: "DELETE", token });
      setItems((prev) => prev.filter((i) => i.listing_id !== listingId));
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not remove.");
    } finally {
      setBusyId(null);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader active="wishlist" />
      <div className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="font-poppins text-2xl font-semibold text-primary-800">Wishlist</h1>
        <p className="mt-1 text-sm text-neutral-500">Books you saved for later.</p>
        <label className="mt-4 flex cursor-pointer items-start gap-3 text-sm text-neutral-700">
          <input
            type="checkbox"
            className="mt-0.5"
            checked={alertsEnabled}
            disabled={prefsBusy}
            onChange={(e) => void toggleAlerts(e.target.checked)}
          />
          <span>
            Email &amp; in-app alerts when a similar CBC book is listed, or a saved book becomes unavailable.
          </span>
        </label>
        {error ? <p className="mt-4 text-sm text-accent-700">{error}</p> : null}
        <ul className="mt-6 space-y-3">
          {items.map((item) => (
            <li key={item.listing_id} className="rounded-xl bg-white p-5 shadow-md">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="font-poppins text-lg font-semibold text-primary-800">
                    {item.listing.title}
                  </h2>
                  <p className="mt-1 text-sm text-neutral-600">
                    {item.listing.grade} · {item.listing.subject} · {item.listing.city}
                    {item.listing.status !== "active" ? ` · ${item.listing.status}` : ""}
                  </p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Link
                    href={`/books/${item.listing_id}`}
                    className="rounded-lg border border-neutral-300 px-3 py-1.5 text-sm text-primary-700"
                  >
                    Open
                  </Link>
                  {item.listing.status === "active" ? (
                    <Link
                      href={`/arrange/${item.listing_id}`}
                      className="rounded-lg bg-primary-700 px-3 py-1.5 text-sm text-white"
                    >
                      Arrange
                    </Link>
                  ) : null}
                  <button
                    type="button"
                    disabled={busyId === item.listing_id}
                    onClick={() => void remove(item.listing_id)}
                    className="rounded-lg border border-neutral-300 px-3 py-1.5 text-sm text-neutral-600"
                  >
                    Remove
                  </button>
                </div>
              </div>
            </li>
          ))}
        </ul>
        {items.length === 0 && !error ? (
          <p className="mt-8 text-neutral-600">
            Nothing saved yet.{" "}
            <Link href="/books" className="text-accent-600">
              Browse books
            </Link>
          </p>
        ) : null}
      </div>
    </main>
  );
}
