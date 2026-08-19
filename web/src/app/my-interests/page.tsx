"use client";

import Link from "next/link";
import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { ApiError, apiFetch } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

type InterestCard = {
  id: string;
  listing_id: string;
  listing_title: string;
  status: string;
  handoff_mode: string;
  city: string;
  created_at_utc: string;
  mtaani_agent_name?: string | null;
};

type InterestPage = { items: InterestCard[] };

function MyInterestsClient() {
  const router = useRouter();
  const search = useSearchParams();
  const sent = search.get("sent");
  const [items, setItems] = useState<InterestCard[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login?returnUrl=/my-interests");
      return;
    }
    (async () => {
      try {
        const page = await apiFetch<InterestPage>("/me/interests?page_size=50", { token });
        setItems(page.items);
      } catch (err) {
        if (err instanceof ApiError) setError(err.problem.message);
        else setError("Could not load interests.");
      }
    })();
  }, [router]);

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader />
      <div className="mx-auto max-w-3xl px-4 py-10">
        <h1 className="font-poppins text-2xl font-semibold text-primary-800">My interests</h1>
        <p className="mt-1 text-sm text-neutral-500">
          Requests you sent. Phones unlock after the seller accepts.
        </p>
        {sent ? (
          <p className="mt-4 rounded-lg border border-secondary-200 bg-secondary-50 px-3 py-2 text-sm text-secondary-700">
            Interest sent. The seller will be notified.
          </p>
        ) : null}
        {error ? <p className="mt-4 text-sm text-accent-700">{error}</p> : null}
        <ul className="mt-6 space-y-3">
          {items.map((item) => (
            <li key={item.id} className="rounded-xl bg-white p-5 shadow-md">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="font-poppins text-lg font-semibold text-primary-800">
                    {item.listing_title}
                  </h2>
                  <p className="mt-1 text-sm text-neutral-600">
                    {item.status} · {item.handoff_mode.replaceAll("_", " ")} · {item.city}
                    {item.mtaani_agent_name ? ` · ${item.mtaani_agent_name}` : ""}
                  </p>
                </div>
                <Link
                  href={`/interests/${item.id}`}
                  className="rounded-lg border border-neutral-300 px-3 py-1.5 text-sm text-primary-700"
                >
                  Open
                </Link>
              </div>
            </li>
          ))}
        </ul>
        {items.length === 0 && !error ? (
          <p className="mt-6 text-neutral-600">
            No requests yet.{" "}
            <Link href="/books" className="text-accent-600 underline">
              Browse books
            </Link>
          </p>
        ) : null}
      </div>
    </main>
  );
}

export default function MyInterestsPage() {
  return (
    <Suspense fallback={<main className="p-8 text-sm text-neutral-500">Loading…</main>}>
      <MyInterestsClient />
    </Suspense>
  );
}
