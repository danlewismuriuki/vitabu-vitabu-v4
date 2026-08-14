const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5080";

export type Problem = {
  error: string;
  message: string;
  errors?: Record<string, string[]> | null;
};

export type UserProfile = {
  id: string;
  display_name: string;
  email: string;
  city: string;
  phone_e164?: string | null;
  phone_verified: boolean;
  phone_verified_at_utc?: string | null;
  email_verified: boolean;
  created_at_utc: string;
};

export type AuthResponse = {
  access_token: string;
  token_type: string;
  expires_in: number;
  user: UserProfile;
};

export class ApiError extends Error {
  constructor(
    public status: number,
    public problem: Problem
  ) {
    super(problem.message);
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit & { token?: string | null } = {}
): Promise<T> {
  const { token, headers, ...rest } = options;
  const res = await fetch(`${API_BASE}${path}`, {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
  });

  if (!res.ok) {
    let problem: Problem = {
      error: "request_failed",
      message: res.statusText || "Request failed",
    };
    try {
      problem = (await res.json()) as Problem;
    } catch {
      /* ignore */
    }
    throw new ApiError(res.status, problem);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

export function fieldErrors(problem: Problem): string[] {
  if (!problem.errors) return [problem.message];
  return Object.values(problem.errors).flat();
}
