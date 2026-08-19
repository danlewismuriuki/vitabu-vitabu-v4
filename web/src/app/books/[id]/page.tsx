import Link from "next/link";
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { apiFetch, ApiError } from "@/lib/api";
import { ReportListingButton } from "@/components/ReportListingButton";
import { WishlistButton } from "@/components/WishlistButton";
import { MessageSellerButton } from "@/components/MessageSellerButton";

type ListingDetail = {
  id: string;
  title: string;
  grade: string;
  subject: string;
  term?: string | null;
  city: string;
  intent: "sale" | "free" | "exchange" | "donate_school";
  condition: string;
  price_kes?: number | null;
  interest_count: number;
  description: string;
  slug: string;
  seller: { display_name: string; city: string };
};

async function getListing(id: string) {
  try {
    return await apiFetch<ListingDetail>(`/listings/${id}`);
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) return null;
    throw err;
  }
}

export async function generateMetadata({
  params,
}: {
  params: Promise<{ id: string }>;
}): Promise<Metadata> {
  const { id } = await params;
  const listing = await getListing(id);
  if (!listing) {
    return { title: "Book not found | Vitabu Vitabu" };
  }
  return {
    title: `${listing.title} · ${listing.city} | Vitabu Vitabu`,
    description: listing.description.slice(0, 155),
    openGraph: {
      title: listing.title,
      description: `${listing.grade} ${listing.subject} in ${listing.city}`,
      type: "website",
    },
  };
}

export default async function BookDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const listing = await getListing(id);
  if (!listing) notFound();

  const price =
    listing.intent === "free"
      ? "Free"
      : listing.intent === "exchange"
        ? "Exchange"
        : listing.intent === "donate_school"
          ? "Donate to school"
          : listing.price_kes != null
            ? `KES ${listing.price_kes.toLocaleString()}`
            : "Price on request";

  return (
    <main className="min-h-screen bg-neutral-50">
      <header className="border-b border-neutral-200 bg-white">
        <div className="mx-auto flex max-w-3xl items-center justify-between px-4 py-5">
          <Link href="/" className="font-poppins text-xl font-bold text-primary-700">
            Vitabu Vitabu
          </Link>
          <Link href="/books" className="text-sm text-accent-600">
            Back to browse
          </Link>
        </div>
      </header>

      <article className="mx-auto max-w-3xl px-4 py-10 animate-fade-in">
        <p className="text-sm uppercase tracking-wide text-accent-600">
          {listing.intent.replace("_", " ")} · {listing.city}
        </p>
        <h1 className="mt-2 font-poppins text-3xl font-bold text-primary-800">
          {listing.title}
        </h1>
        <p className="mt-3 text-neutral-600">
          {listing.grade} · {listing.subject}
          {listing.term ? ` · ${listing.term}` : ""} · {listing.condition.replaceAll("_", " ")}
        </p>
        <p className="mt-4 font-poppins text-2xl font-semibold text-primary-700">{price}</p>
        {listing.interest_count > 0 ? (
          <p className="mt-2 text-sm text-neutral-500">{listing.interest_count} interested</p>
        ) : null}

        <div className="mt-8 rounded-xl bg-white p-6 shadow-md">
          <h2 className="font-poppins text-lg font-semibold text-primary-800">About this book</h2>
          <p className="mt-3 whitespace-pre-wrap text-neutral-700">{listing.description}</p>
        </div>

        <div className="mt-6 rounded-xl bg-white p-6 shadow-md">
          <h2 className="font-poppins text-lg font-semibold text-primary-800">Seller</h2>
          <p className="mt-2 text-neutral-700">
            {listing.seller.display_name} · {listing.seller.city}
          </p>
          <p className="mt-1 text-sm text-neutral-500">
            Phone is shared only after both parties accept a deal.
          </p>
        </div>

        <div className="mt-8 flex flex-wrap gap-3">
          <Link href={`/arrange/${listing.id}`} className="btn-primary">
            Arrange / request
          </Link>
          <MessageSellerButton listingId={listing.id} />
          <WishlistButton listingId={listing.id} />
          <Link href="/books" className="btn-secondary">
            Keep browsing
          </Link>
        </div>
        <ReportListingButton listingId={listing.id} />
      </article>
    </main>
  );
}
