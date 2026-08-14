export default function App() {
  return (
    <div className="min-h-screen bg-neutral-50 font-lato text-neutral-800">
      <header className="border-b border-neutral-200 bg-white px-6 py-4">
        <p className="font-poppins text-xl font-bold text-primary-700">
          Vitabu Vitabu Admin
        </p>
        <p className="text-sm text-neutral-500">
          Platform ops — moderate listings, users, CBC catalog
        </p>
      </header>
      <main className="mx-auto max-w-4xl px-6 py-12">
        <h1 className="font-poppins text-2xl font-semibold text-primary-800">
          Staff shell (S0)
        </h1>
        <p className="mt-3 max-w-xl text-neutral-600">
          Login and permission-guarded routes arrive in later slices. Parents
          never use this app — marketplace lives in <code>web/</code>.
        </p>
        <a
          className="mt-8 inline-flex rounded-lg bg-accent-500 px-5 py-3 font-medium text-white hover:bg-accent-600"
          href="http://localhost:5080/health"
          target="_blank"
          rel="noreferrer"
        >
          Check API health
        </a>
      </main>
    </div>
  );
}
