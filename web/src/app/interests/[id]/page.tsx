"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { FormError } from "@/components/AuthShell";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";
import { getStoredUser, getToken } from "@/lib/auth-storage";

type Party = {
  id: string;
  display_name: string;
  city: string;
  phone_e164?: string | null;
};

type InterestDetail = {
  id: string;
  listing_id: string;
  listing_title: string;
  status: string;
  handoff_mode: string;
  city: string;
  message?: string | null;
  reserved_until_utc?: string | null;
  buyer: Party;
  seller: Party;
};

export default function InterestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [detail, setDetail] = useState<InterestDetail | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);
  const me = getStoredUser();

  async function load() {
    const token = getToken();
    if (!token) {
      router.replace(`/login?returnUrl=/interests/${id}`);
      return;
    }
    try {
      const data = await apiFetch<InterestDetail>(`/interests/${id}`, { token });
      setDetail(data);
      setErrors([]);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Could not load deal."]);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, router]);

  async function act(path: string) {
    const token = getToken();
    setBusy(true);
    try {
      const data = await apiFetch<InterestDetail>(`/interests/${id}/${path}`, {
        method: "POST",
        token,
      });
      setDetail(data);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
    } finally {
      setBusy(false);
    }
  }

  const isSeller = detail && me && detail.seller.id === me.id;
  const isBuyer = detail && me && detail.buyer.id === me.id;

  return (
    <main className="min-h-screen bg-neutral-50">
      <SiteHeader />
      <div className="mx-auto max-w-xl px-4 py-10">
        <Link href={isSeller ? "/my-listings" : "/my-interests"} className="text-sm text-accent-600">
          Back
        </Link>
        <FormError messages={errors} />
        {!detail ? (
          <p className="mt-6 text-sm text-neutral-500">Loading…</p>
        ) : (
          <div className="mt-4 rounded-xl bg-white p-6 shadow-md animate-fade-in">
            <h1 className="font-poppins text-2xl font-semibold text-primary-800">
              {detail.listing_title}
            </h1>
            <p className="mt-2 text-sm uppercase tracking-wide text-neutral-500">
              {detail.status} · {detail.handoff_mode.replaceAll("_", " ")} · {detail.city}
            </p>
            {detail.message ? (
              <p className="mt-4 text-neutral-700 whitespace-pre-wrap">{detail.message}</p>
            ) : null}

            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              <div className="rounded-lg bg-neutral-50 p-3 text-sm">
                <p className="font-medium text-primary-800">Buyer</p>
                <p>{detail.buyer.display_name}</p>
                <p className="text-neutral-500">{detail.buyer.city}</p>
                {detail.buyer.phone_e164 ? (
                  <p className="mt-2 font-medium text-accent-700">{detail.buyer.phone_e164}</p>
                ) : null}
              </div>
              <div className="rounded-lg bg-neutral-50 p-3 text-sm">
                <p className="font-medium text-primary-800">Seller</p>
                <p>{detail.seller.display_name}</p>
                <p className="text-neutral-500">{detail.seller.city}</p>
                {detail.seller.phone_e164 ? (
                  <p className="mt-2 font-medium text-accent-700">{detail.seller.phone_e164}</p>
                ) : null}
              </div>
            </div>

            {detail.status === "accepted" && detail.reserved_until_utc ? (
              <p className="mt-4 text-xs text-neutral-500">
                Reserved until {new Date(detail.reserved_until_utc).toLocaleString()}
              </p>
            ) : null}

            <div className="mt-6 flex flex-wrap gap-2">
              {isSeller && (detail.status === "pending" || detail.status === "waitlisted") ? (
                <>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => act("accept")}
                    className="btn-primary !py-2 text-sm"
                  >
                    Accept
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => act("decline")}
                    className="btn-secondary !py-2 text-sm"
                  >
                    Decline
                  </button>
                </>
              ) : null}
              {isBuyer && (detail.status === "pending" || detail.status === "waitlisted") ? (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => act("cancel")}
                  className="btn-secondary !py-2 text-sm"
                >
                  Cancel request
                </button>
              ) : null}
              {detail.status === "accepted" ? (
                <>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => act("complete")}
                    className="btn-primary !py-2 text-sm"
                  >
                    Mark complete
                  </button>
                  <button
                    type="button"
                    disabled={busy}
                    onClick={() => act("release")}
                    className="btn-secondary !py-2 text-sm"
                  >
                    Release reserve
                  </button>
                </>
              ) : null}
              <Link href={`/books/${detail.listing_id}`} className="btn-secondary !py-2 text-sm">
                Listing
              </Link>
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
