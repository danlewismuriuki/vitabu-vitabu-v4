"use client";

import { FormEvent, useState } from "react";
import { ApiError, apiFetch, fieldErrors } from "@/lib/api";
import { getToken } from "@/lib/auth-storage";

const REASONS = [
  { value: "spam_or_scam", label: "Spam or scam" },
  { value: "condition_not_as_described", label: "Condition not as described" },
  { value: "counterfeit_or_photocopy", label: "Photocopy / counterfeit" },
  { value: "child_or_pii_in_photo", label: "Child / private info in photo" },
  { value: "other", label: "Other" },
];

export function ReportListingButton({ listingId }: { listingId: string }) {
  const [open, setOpen] = useState(false);
  const [reason, setReason] = useState(REASONS[0].value);
  const [details, setDetails] = useState("");
  const [msg, setMsg] = useState<string | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  async function submit(e: FormEvent) {
    e.preventDefault();
    const token = getToken();
    if (!token) {
      window.location.href = `/login?returnUrl=/books/${listingId}`;
      return;
    }
    setLoading(true);
    setErrors([]);
    try {
      await apiFetch(`/listings/${listingId}/reports`, {
        method: "POST",
        token,
        body: JSON.stringify({ reason, details: details || null }),
      });
      setMsg("Thanks — staff will review this report.");
      setOpen(false);
    } catch (err) {
      if (err instanceof ApiError) setErrors(fieldErrors(err.problem));
      else setErrors(["Could not send report."]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="mt-4">
      {msg ? <p className="text-sm text-secondary-700">{msg}</p> : null}
      {!open ? (
        <button
          type="button"
          onClick={() => setOpen(true)}
          className="text-sm text-neutral-500 underline hover:text-accent-600"
        >
          Report this listing
        </button>
      ) : (
        <form onSubmit={submit} className="rounded-lg border border-neutral-200 bg-white p-4 text-sm">
          {errors.length ? (
            <ul className="mb-2 list-disc pl-4 text-accent-700">
              {errors.map((e) => (
                <li key={e}>{e}</li>
              ))}
            </ul>
          ) : null}
          <label className="block font-medium text-primary-700">Reason</label>
          <select
            className="mt-1 w-full rounded-lg border border-neutral-300 px-3 py-2"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
          >
            {REASONS.map((r) => (
              <option key={r.value} value={r.value}>
                {r.label}
              </option>
            ))}
          </select>
          <label className="mt-3 block font-medium text-primary-700">Details</label>
          <textarea
            className="mt-1 w-full rounded-lg border border-neutral-300 px-3 py-2"
            rows={2}
            value={details}
            onChange={(e) => setDetails(e.target.value)}
          />
          <div className="mt-3 flex gap-2">
            <button type="submit" disabled={loading} className="btn-primary !py-2 text-sm">
              {loading ? "Sending…" : "Submit report"}
            </button>
            <button
              type="button"
              className="btn-secondary !py-2 text-sm"
              onClick={() => setOpen(false)}
            >
              Cancel
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
