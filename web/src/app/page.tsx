import Link from "next/link";

export default function HomePage() {
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
          <Link href="/login" className="btn-secondary !px-4 !py-2 text-sm">
            Log in
          </Link>
        </nav>
      </header>

      <section className="relative mx-auto flex max-w-6xl flex-col gap-8 px-4 pb-24 pt-10 md:pt-20">
        <div className="max-w-2xl animate-fade-in">
          <p className="mb-3 font-poppins text-sm font-medium uppercase tracking-wide text-accent-600">
            Built for Kenya
          </p>
          <h1 className="font-poppins text-4xl font-bold leading-tight text-primary-800 md:text-5xl">
            Real Parents. Real Savings. Real Books.
          </h1>
          <p className="mt-5 max-w-xl text-lg text-neutral-600">
            Circulate CBC schoolbooks with other parents — sell, give free, or
            exchange. Meet nearby or use Pickup Mtaani.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link href="/books" className="btn-primary">
              Browse books
            </Link>
            <Link href="/sell" className="btn-secondary">
              List a book
            </Link>
            <Link href="/signup" className="btn-secondary">
              Sign up
            </Link>
          </div>
        </div>

        <p className="animate-slide-up text-sm text-neutral-500">
          S1 — auth pages live at /signup, /login, /verify-phone.
        </p>
      </section>
    </main>
  );
}
