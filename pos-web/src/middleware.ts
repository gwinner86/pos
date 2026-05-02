import { NextResponse } from 'next/server'
import type { NextRequest } from 'next/server'

// This function can be marked `async` if using `await` inside
export function middleware(request: NextRequest) {
    // Current implementation relies on client-side check in AuthProvider for redirect.
    // Ideally, valid JWT should be in cookies for this to work robustly on server.
    // For now, we will allow the request but can add basic path checks.

    // Example: If we had a cookie named 'token'
    // const token = request.cookies.get('token')?.value

    // if (request.nextUrl.pathname.startsWith('/dashboard') && !token) {
    //   return NextResponse.redirect(new URL('/login', request.url))
    // }

    return NextResponse.next()
}

export const config = {
    matcher: '/dashboard/:path*',
}
