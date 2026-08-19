"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type InterestCard = {
  id: string;
  status: string;
  handoff_mode: string;
  city: string;
  buyer_display_name: string;
  created_at_utc: string;
};

type InterestPage = { items: InterestCard[]; listing_title?: string };

export default function ListingInterestsPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [items, setItems] = useState<InterestCard[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  async function load() {
    const token = getToken();
    if (!token) {
      router.replace(`/login?returnUrl=/my-listings/${id}/interests`);
      return;
    }
    try {
      const page = await apiFetch<InterestPage>(`/me/listings/${id}/interests?page_size=50`, {
        token,
      });
      setItems(page.items);
      setError(null);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
      else setError("Could not load interests.");
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, router]);

  async function accept(interestId: string) {
    const token = getToken();
    setBusyId(interestId);
    try {
      await apiFetch(`/interests/${interestId}/accept`, { method: "POST", token });
      router.push(`/interests/${interestId}`);
    } catch (err) {
      if (err instanceof ApiError) setError(err.problem.message);
    } finally {
      setBusyId(null);
    }
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader active="mine" />
      <div className="mx-auto max-w-3xl px-4 py-10">
        <Link href="/my-listings" className="text-sm text-accent-600">
          Back to my listings
        </Link>
        <h1 className="mt-4 font-poppins text-2xl font-semibold text-primary-800">
          Interests
        </h1>
        <p className="mt-1 text-sm text-neutral-500">
          Accept one buyer to reserve the book. Others are waitlisted.
        </p>
        {error ? <p className="mt-4 text-sm text-accent-700">{error}</p> : null}
        <ul className="mt-6 space-y-3">
          {items.map((item) => (
            <li key={item.id} className="rounded-xl bg-white p-5 shadow-md">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="font-poppins text-lg font-semibold text-primary-800">
                    {item.buyer_display_name}
                  </h2>
                  <p className="mt-1 text-sm text-neutral-600">
                    {item.status} · {item.handoff_mode.replaceAll("_", " ")} · {item.city}
                  </p>
                </div>
                <div className="flex flex-wrap gap-2 text-sm">
                  <Link
                    href={`/interests/${item.id}`}
                    className="rounded-lg border border-neutral-300 px-3 py-1.5 text-primary-700"
                  >
                    Open
                  </Link>
                  {(item.status === "pending" || item.status === "waitlisted") ? (
                    <button
                      type="button"
                      disabled={busyId === item.id}
                      onClick={() => accept(item.id)}
                      className="rounded-lg bg-accent-500 px-3 py-1.5 text-white"
                    >
                      Accept
                    </button>
                  ) : null}
                </div>
              </div>
            </li>
          ))}
        </ul>
        {items.length === 0 && !error ? (
          <p className="mt-6 text-neutral-600">No interest yet on this listing.</p>
        ) : null}
      </div>
    </main>
  );
}
