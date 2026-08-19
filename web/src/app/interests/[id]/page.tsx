"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/SiteHeader";
import { FormError, fieldClass, labelClass } from "@/components/AuthShell";
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
  buyer_completed_at_utc?: string | null;
  seller_completed_at_utc?: string | null;
  dispute_reason?: string | null;
  buyer: Party;
  seller: Party;
  mtaani_agent?: {
    id: number;
    business_name: string;
    location_id?: number | null;
    location_name?: string | null;
    estimated_fee_kes?: number | null;
  } | null;
};

export default function InterestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [detail, setDetail] = useState<InterestDetail | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);
  const [disputeReason, setDisputeReason] = useState("");
  const [stars, setStars] = useState(5);
  const [rateComment, setRateComment] = useState("");
  const [rated, setRated] = useState(false);
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

  async function act(path: string, body?: unknown) {
    const token = getToken();
    setBusy(true);
    try {
      const data = await apiFetch<InterestDetail>(`/interests/${id}/${path}`, {
        method: "POST",
        token,
        body: body ? JSON.stringify(body) : undefined,
      });
      setDetail(data);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
    } finally {
      setBusy(false);
    }
  }

  async function submitRate(e: FormEvent) {
    e.preventDefault();
    const token = getToken();
    setBusy(true);
    try {
      await apiFetch(`/interests/${id}/rate`, {
        method: "POST",
        token,
        body: JSON.stringify({ stars, comment: rateComment || null }),
      });
      setRated(true);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
    } finally {
      setBusy(false);
    }
  }

  const isSeller = detail && me && detail.seller.id === me.id;
  const isBuyer = detail && me && detail.buyer.id === me.id;
  const iConfirmed =
    detail &&
    ((isBuyer && detail.buyer_completed_at_utc) ||
      (isSeller && detail.seller_completed_at_utc));

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
            {detail.mtaani_agent ? (
              <div className="mt-4 rounded-lg bg-primary-50 px-3 py-3 text-sm text-primary-800">
                <p className="font-medium">{detail.mtaani_agent.business_name}</p>
                <p className="text-neutral-600">
                  Pickup Mtaani
                  {detail.mtaani_agent.location_name
                    ? ` · ${detail.mtaani_agent.location_name}`
                    : ""}
                  {detail.mtaani_agent.estimated_fee_kes != null
                    ? ` · est. KES ${detail.mtaani_agent.estimated_fee_kes}`
                    : ""}
                </p>
              </div>
            ) : null}
            {detail.dispute_reason ? (
              <p className="mt-3 rounded-lg bg-accent-50 px-3 py-2 text-sm text-accent-700">
                Dispute: {detail.dispute_reason}
              </p>
            ) : null}
            {detail.message ? (
              <p className="mt-4 whitespace-pre-wrap text-neutral-700">{detail.message}</p>
            ) : null}

            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              <div className="rounded-lg bg-neutral-50 p-3 text-sm">
                <p className="font-medium text-primary-800">Buyer</p>
                <p>{detail.buyer.display_name}</p>
                <p className="text-neutral-500">{detail.buyer.city}</p>
                {detail.buyer.phone_e164 ? (
                  <p className="mt-2 font-medium text-accent-700">{detail.buyer.phone_e164}</p>
                ) : null}
                {detail.buyer_completed_at_utc ? (
                  <p className="mt-1 text-xs text-secondary-700">Confirmed complete</p>
                ) : null}
              </div>
              <div className="rounded-lg bg-neutral-50 p-3 text-sm">
                <p className="font-medium text-primary-800">Seller</p>
                <p>{detail.seller.display_name}</p>
                <p className="text-neutral-500">{detail.seller.city}</p>
                {detail.seller.phone_e164 ? (
                  <p className="mt-2 font-medium text-accent-700">{detail.seller.phone_e164}</p>
                ) : null}
                {detail.seller_completed_at_utc ? (
                  <p className="mt-1 text-xs text-secondary-700">Confirmed complete</p>
                ) : null}
              </div>
            </div>

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
              {(detail.status === "accepted" || detail.status === "disputed") && !iConfirmed ? (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => act("complete")}
                  className="btn-primary !py-2 text-sm"
                >
                  Confirm handoff
                </button>
              ) : null}
              {detail.status === "accepted" ? (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => act("release")}
                  className="btn-secondary !py-2 text-sm"
                >
                  Release reserve
                </button>
              ) : null}
              <Link href={`/books/${detail.listing_id}`} className="btn-secondary !py-2 text-sm">
                Listing
              </Link>
            </div>

            {(detail.status === "accepted" || detail.status === "disputed") ? (
              <form
                className="mt-6 space-y-2 border-t border-neutral-100 pt-4"
                onSubmit={(e) => {
                  e.preventDefault();
                  void act("dispute", { reason: disputeReason });
                }}
              >
                <label className={labelClass} htmlFor="dispute">
                  Open dispute
                </label>
                <input
                  id="dispute"
                  required
                  className={fieldClass}
                  value={disputeReason}
                  onChange={(e) => setDisputeReason(e.target.value)}
                  placeholder="What went wrong?"
                />
                <button type="submit" disabled={busy} className="btn-secondary !py-2 text-sm">
                  Submit dispute
                </button>
              </form>
            ) : null}

            {detail.status === "completed" && !rated ? (
              <form onSubmit={submitRate} className="mt-6 space-y-2 border-t border-neutral-100 pt-4">
                <p className={labelClass}>Rate the other parent</p>
                <select
                  className={fieldClass}
                  value={stars}
                  onChange={(e) => setStars(Number(e.target.value))}
                >
                  {[5, 4, 3, 2, 1].map((n) => (
                    <option key={n} value={n}>
                      {n} stars
                    </option>
                  ))}
                </select>
                <textarea
                  className={fieldClass}
                  rows={2}
                  value={rateComment}
                  onChange={(e) => setRateComment(e.target.value)}
                  placeholder="Optional comment"
                />
                <button type="submit" disabled={busy} className="btn-primary !py-2 text-sm">
                  Submit rating
                </button>
              </form>
            ) : null}
            {rated ? (
              <p className="mt-4 text-sm text-secondary-700">Thanks for your rating.</p>
            ) : null}
          </div>
        )}
      </div>
    </main>
  );
}
