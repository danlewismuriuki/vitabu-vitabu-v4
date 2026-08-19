import Link from "next/link";
import { apiFetch } from "@/lib/api";
import { ListingCard, ListingCardView } from "@/components/ListingCardView";

type ListingPage = {
  items: ListingCard[];
  page: number;
  page_size: number;
  total_items: number;
  total_pages: number;
};

async function loadDonateListings() {
  return apiFetch<ListingPage>("/listings?intent=donate_school&page_size=24");
}

export const metadata = {
  title: "Donate books to schools | Vitabu Vitabu",
  description: "CBC books listed for donation to Kenyan schools.",
};

export default async function DonatePage() {
  let page: ListingPage = {
    items: [],
    page: 1,
    page_size: 24,
    total_items: 0,
    total_pages: 0,
  };
  let error: string | null = null;
  try {
    page = await loadDonateListings();
  } catch {
    error = "Could not load donate listings. Is the API running on :5080?";
  }

  return (
    <main className="min-h-screen bg-neutral-50">
      <header className="border-b border-neutral-200 bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-5">
          <Link href="/" className="font-poppins text-xl font-bold text-primary-700">
            Vitabu Vitabu
          </Link>
          <nav className="flex gap-3 text-sm">
            <Link href="/books" className="text-primary-700 hover:text-accent-600">
              Browse all
            </Link>
            <Link href="/sell" className="text-accent-600">
              List a donate book
            </Link>
          </nav>
        </div>
      </header>

      <div className="mx-auto max-w-6xl px-4 py-10">
        <h1 className="font-poppins text-3xl font-bold text-primary-800">Donate to schools</h1>
        <p className="mt-2 max-w-2xl text-neutral-600">
          Parents listing books for school drives. Arrange handoff the same way as free giveaways —
          school profiles and campaigns come later.
        </p>
        {error ? <p className="mt-6 text-sm text-accent-700">{error}</p> : null}
        <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {page.items.map((listing) => (
            <ListingCardView key={listing.id} listing={listing} />
          ))}
        </div>
        {page.items.length === 0 && !error ? (
          <p className="mt-8 text-neutral-600">
            No donate listings yet.{" "}
            <Link href="/sell" className="text-accent-600">
              Be the first
            </Link>
          </p>
        ) : null}
      </div>
    </main>
  );
}
