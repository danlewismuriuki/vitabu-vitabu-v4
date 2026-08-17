"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ListingCard } from "@/components/ListingCardView";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type ListingPage = {
  items: ListingCard[];
  total_items: number;
};

export default function MyListingsClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const published = searchParams.get("published");
  const [items, setItems] = useState<ListingCard[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  async function load() {
    const token = getToken();
    if (!token) {
      router.replace("/login?returnUrl=/my-listings");
      return;
    }
    try {
      const page = await apiFetch<ListingPage>("/me/listings?page_size=50", { token });
      setItems(page.items);
      setError(null);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not load your listings.");
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [router]);

  async function pause(id: string) {
    const token = getToken();
    setBusyId(id);
    try {
      await apiFetch(`/listings/${id}/pause`, { method: "POST", token });
      await load();
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
    } finally {
      setBusyId(null);
    }
  }

  async function resume(id: string) {
    const token = getToken();
    setBusyId(id);
    try {
      await apiFetch(`/listings/${id}/resume`, { method: "POST", token });
      await load();
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
    } finally {
      setBusyId(null);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader active="mine" />
      <div className="mx-auto max-w-4xl px-4 py-10">
        <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
          <div>
            <h1 className="font-poppins text-2xl font-semibold text-primary-800">
              My listings
            </h1>
            <p className="mt-1 text-sm text-neutral-500">
              Pause to hide from Browse. Edit anytime while active or paused.
            </p>
          </div>
          <Link href="/sell" className="btn-primary !py-2.5 text-sm">
            List a book
          </Link>
        </div>

        {published ? (
          <div className="mb-4 rounded-lg border border-secondary-200 bg-secondary-50 px-3 py-2 text-sm text-secondary-700 animate-fade-in">
            Listing published.{" "}
            <Link href={`/books/${published}`} className="underline">
              View on Browse
            </Link>
          </div>
        ) : null}

        {error ? <p className="mb-4 text-sm text-accent-700">{error}</p> : null}

        {items.length === 0 ? (
          <p className="text-neutral-600">
            No listings yet.{" "}
            <Link href="/sell" className="text-accent-600 underline">
              Sell your first book
            </Link>
            .
          </p>
        ) : (
          <ul className="space-y-3">
            {items.map((listing) => (
              <li
                key={listing.id}
                className="rounded-xl bg-white p-5 shadow-md animate-fade-in"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h2 className="font-poppins text-lg font-semibold text-primary-800">
                      {listing.title}
                    </h2>
                    <p className="mt-1 text-sm text-neutral-600">
                      {listing.grade} · {listing.subject} · {listing.city}
                    </p>
                    <p className="mt-2 text-xs uppercase tracking-wide text-neutral-500">
                      {listing.status} · {listing.intent}
                      {listing.price_kes != null
                        ? ` · KES ${listing.price_kes.toLocaleString()}`
                        : ""}
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-2 text-sm">
                    <Link
                      href={`/my-listings/${listing.id}/edit`}
                      className="rounded-lg border border-neutral-300 px-3 py-1.5 text-primary-700 hover:bg-neutral-50"
                    >
                      Edit
                    </Link>
                    {listing.status === "active" ? (
                      <>
                        <Link
                          href={`/books/${listing.id}`}
                          className="rounded-lg border border-neutral-300 px-3 py-1.5 text-primary-700 hover:bg-neutral-50"
                        >
                          View
                        </Link>
                        <button
                          type="button"
                          disabled={busyId === listing.id}
                          onClick={() => pause(listing.id)}
                          className="rounded-lg border border-neutral-300 px-3 py-1.5 text-primary-700 hover:bg-neutral-50"
                        >
                          Pause
                        </button>
                      </>
                    ) : null}
                    {listing.status === "paused" ? (
                      <button
                        type="button"
                        disabled={busyId === listing.id}
                        onClick={() => resume(listing.id)}
                        className="rounded-lg bg-accent-500 px-3 py-1.5 text-white hover:bg-accent-600"
                      >
                        Resume
                      </button>
                    ) : null}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </main>
  );
}
