"use client";

import Link from "next/link";

export type ListingCard = {
  id: string;
  title: string;
  grade: string;
  subject: string;
  term?: string | null;
  city: string;
  intent: "sale" | "free" | "exchange";
  condition: string;
  status: string;
  price_kes?: number | null;
  cover_image_url?: string | null;
  interest_count: number;
  created_at_utc: string;
};

const intentLabel: Record<ListingCard["intent"], string> = {
  sale: "For sale",
  free: "Free",
  exchange: "Exchange",
};

const intentClass: Record<ListingCard["intent"], string> = {
  sale: "bg-accent-100 text-accent-700",
  free: "bg-secondary-100 text-secondary-700",
  exchange: "bg-gold-100 text-gold-700",
};

function priceLabel(listing: ListingCard) {
  if (listing.intent === "free") return "Free";
  if (listing.intent === "exchange") return "Exchange";
  if (listing.price_kes != null) return `KES ${listing.price_kes.toLocaleString()}`;
  return "Price on request";
}

export function ListingCardView({ listing }: { listing: ListingCard }) {
  return (
    <Link
      href={`/books/${listing.id}`}
      className="block rounded-xl bg-white p-5 shadow-md transition hover:shadow-lg animate-fade-in"
    >
      <div className="flex items-start justify-between gap-3">
        <h2 className="font-poppins text-lg font-semibold text-primary-800">
          {listing.title}
        </h2>
        <span
          className={`shrink-0 rounded-full px-3 py-1 text-xs font-medium ${intentClass[listing.intent]}`}
        >
          {intentLabel[listing.intent]}
        </span>
      </div>
      <p className="mt-2 text-sm text-neutral-600">
        {listing.grade} · {listing.subject}
        {listing.term ? ` · ${listing.term}` : ""}
      </p>
      <div className="mt-4 flex flex-wrap items-center gap-3 text-sm">
        <span className="rounded-md bg-accent-50 px-2 py-1 text-accent-700">
          {listing.city}
        </span>
        <span className="font-medium text-primary-700">{priceLabel(listing)}</span>
        {listing.interest_count > 0 ? (
          <span className="text-neutral-500">{listing.interest_count} interested</span>
        ) : null}
      </div>
    </Link>
  );
}
