import Link from "next/link";
import { apiFetch } from "@/lib/api";
import { ListingCard, ListingCardView } from "@/components/ListingCardView";

type ListingPage = {
  items: ListingCard[];
  total_items: number;
};

async function loadFeatured() {
  try {
    return await apiFetch<ListingPage>("/listings?page_size=6");
  } catch {
    return { items: [], total_items: 0 };
  }
}

export default async function HomePage() {
  const featured = await loadFeatured();

  return (
    <main className="relative min-h-screen overflow-hidden bg-kitenge-pattern">
      <div className="pointer-events-none absolute inset-0 bg-gradient-to-b from-neutral-50/95 via-neutral-50/90 to-accent-50/40" />

      <header className="relative mx-auto flex max-w-6xl items-center justify-between px-4 py-6">
        <p className="font-poppins text-2xl font-bold tracking-tight text-primary-700 md:text-3xl">
          Vitabu Vitabu
        </p>
        <nav className="flex items-center gap-3 text-sm">
          <Link href="/books" className="text-primary-700 hover:text-accent-600">
            Browse
          </Link>
          <Link href="/donate" className="text-primary-700 hover:text-accent-600">
            Donate
          </Link>
          <Link href="/sell" className="text-primary-700 hover:text-accent-600">
            Sell
          </Link>
          <Link href="/login" className="btn-secondary !px-4 !py-2 text-sm">
            Log in
          </Link>
        </nav>
      </header>

      <section className="relative mx-auto flex max-w-6xl flex-col gap-8 px-4 pb-16 pt-10 md:pt-16">
        <div className="max-w-2xl animate-fade-in">
          <p className="mb-3 font-poppins text-sm font-medium uppercase tracking-wide text-accent-600">
            Built for Kenya
          </p>
          <h1 className="font-poppins text-4xl font-bold leading-tight text-primary-800 md:text-5xl">
            Real Parents. Real Savings. Real Books.
          </h1>
          <p className="mt-5 max-w-xl text-lg text-neutral-600">
            Circulate CBC schoolbooks with other parents — sell, give free,
            exchange, or donate to schools. Meet nearby or use Pickup Mtaani.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link href="/books" className="btn-primary">
              Browse books
            </Link>
            <Link href="/donate" className="btn-secondary">
              Donate hub
            </Link>
            <Link href="/sell" className="btn-secondary">
              Sell a book
            </Link>
          </div>
        </div>
      </section>

      {featured.items.length > 0 ? (
        <section className="relative mx-auto max-w-6xl px-4 pb-20">
          <div className="mb-4 flex items-end justify-between gap-4">
            <h2 className="font-poppins text-2xl font-semibold text-primary-800">
              Fresh listings
            </h2>
            <Link href="/books" className="text-sm text-accent-600 hover:text-accent-700">
              See all ({featured.total_items})
            </Link>
          </div>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {featured.items.map((listing) => (
              <ListingCardView key={listing.id} listing={listing} />
            ))}
          </div>
        </section>
      ) : null}
    </main>
  );
}
