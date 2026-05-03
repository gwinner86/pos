"use client";

import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import Link from "next/link";
import dynamic from "next/dynamic";
import { cn } from "@/lib/utils";
import {
    Menu,
    LogOut,
    Search,
    Bell,
    Moon,
    Sun
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    Sheet,
    SheetContent,
    SheetTrigger,
} from "@/components/ui/sheet";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Input } from "@/components/ui/input";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb"
import { useAuth } from "@/hooks/use-auth";
import { useTheme } from "next-themes";
import { navItems } from "./nav-items";

const Sidebar = dynamic(() => import("./sidebar").then(mod => mod.Sidebar), { ssr: false });

export function Header({
    isSidebarCollapsed,
    onMobileMenuOpen
}: {
    isSidebarCollapsed: boolean;
    onMobileMenuOpen: () => void
}) {
    const { logout, user, selectedCompany, selectedLocation, setSelectedLocation } = useAuth();
    const { setTheme, theme } = useTheme();
    const pathname = usePathname();
    const [locations, setLocations] = useState<any[]>([]);
    const [isLoadingLocations, setIsLoadingLocations] = useState(false);

    useEffect(() => {
        if (!selectedCompany) return;
        const fetchLocations = async () => {
            setIsLoadingLocations(true);
            try {
                // Same endpoint used by select-location
                // We're importing api locally to avoid cyclic refs if needed, but normally api is in @/lib/api
                const api = (await import("@/lib/api")).default;
                const response = await api.get(`/api/Users/me/companies/${selectedCompany.companyId}/locations`);
                setLocations(response.data.data || []);
            } catch (err) {
                console.error("Failed to fetch locations for switcher", err);
            } finally {
                setIsLoadingLocations(false);
            }
        };
        fetchLocations();
    }, [selectedCompany]);

    const handleLocationSwitch = (location: any) => {
        if (location.id === selectedLocation?.id) return;
        setSelectedLocation(location);
        // Force reload so all the react query hooks and data grids refresh against the new branch
        window.location.reload();
    };

    const generateBreadcrumbs = () => {
        const paths = pathname.split('/').filter(path => path);
        // Map paths to readable names if needed, or capitalize
        return paths.map((path, index) => {
            const href = `/${paths.slice(0, index + 1).join('/')}`;
            const title = path.charAt(0).toUpperCase() + path.slice(1);
            return { title, href };
        });
    };

    const breadcrumbs = generateBreadcrumbs();

    return (
        <header
            className={cn(
                "fixed top-0 z-30 flex h-16 items-center gap-4 border-b bg-background px-6 shadow-sm transition-all duration-300",
                isSidebarCollapsed ? "left-[70px] w-[calc(100%-70px)]" : "left-64 w-[calc(100%-16rem)]",
                "max-md:left-0 max-md:w-full"
            )}
        >
            <Sheet>
                <SheetTrigger asChild>
                    <Button variant="outline" size="icon" className="shrink-0 md:hidden" onClick={onMobileMenuOpen}>
                        <Menu className="h-5 w-5" />
                        <span className="sr-only">Toggle navigation menu</span>
                    </Button>
                </SheetTrigger>
                <SheetContent side="left" className="w-64 p-0">
                    <div className="flex h-16 items-center border-b px-6">
                        <Link href="/dashboard" className="flex items-center gap-2 font-semibold">
                            <div className="h-8 w-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground font-bold">
                                P
                            </div>
                            <span className="text-lg">POS System</span>
                        </Link>
                    </div>
                    <nav className="grid gap-1 px-2 py-4">
                        {navItems.map((item, index) => {
                            const Icon = item.icon;
                            return (
                                <Link
                                    key={index}
                                    href={item.href}
                                    className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-all hover:bg-accent hover:text-accent-foreground"
                                >
                                    <Icon className="h-5 w-5" />
                                    <span>{item.title}</span>
                                </Link>
                            )
                        })}
                    </nav>
                </SheetContent>
            </Sheet>



            <div className="w-full flex-1 md:w-auto md:flex-none">
                <Breadcrumb className="hidden md:flex">
                    <BreadcrumbList>
                        {breadcrumbs.map((crumb, index) => (
                            <div key={crumb.href} className="flex items-center">
                                <BreadcrumbItem>
                                    {index === breadcrumbs.length - 1 ? (
                                        <BreadcrumbPage>{crumb.title}</BreadcrumbPage>
                                    ) : (
                                        <BreadcrumbLink asChild>
                                            <Link href={crumb.href}>{crumb.title}</Link>
                                        </BreadcrumbLink>
                                    )}
                                </BreadcrumbItem>
                                {index < breadcrumbs.length - 1 && <BreadcrumbSeparator />}
                            </div>
                        ))}
                    </BreadcrumbList>
                </Breadcrumb>
            </div>

            <div className="w-full flex-1 md:w-auto md:flex-none md:ml-auto">
                <form>
                    <div className="relative">
                        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                        <Input
                            type="search"
                            placeholder="Search products, orders, customers..."
                            className="w-full bg-background pl-8 md:w-[300px] lg:w-[400px]"
                        />
                    </div>
                </form>
            </div>

            <div className="hidden md:flex items-center mx-2 px-3 py-1.5 bg-primary/10 text-primary rounded-full text-sm font-medium hover:bg-primary/20 cursor-pointer transition-colors">
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <div className="flex items-center gap-2">
                            {isLoadingLocations ? (
                                <span className="animate-pulse">Loading branch...</span>
                            ) : (
                                selectedLocation?.locationName || "No Branch Selected"
                            )}
                        </div>
                    </DropdownMenuTrigger>
                    {locations.length > 0 && (
                        <DropdownMenuContent align="end" className="w-56">
                            <DropdownMenuLabel>Switch Branch</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            {locations.map((loc) => (
                                <DropdownMenuItem
                                    key={loc.id}
                                    onClick={() => handleLocationSwitch(loc)}
                                    className={loc.id === selectedLocation?.id ? "bg-accent" : ""}
                                >
                                    <div className="flex flex-col">
                                        <span className="font-medium">{loc.locationName}</span>
                                        <span className="text-xs text-muted-foreground">{loc.locationType}</span>
                                    </div>
                                </DropdownMenuItem>
                            ))}
                        </DropdownMenuContent>
                    )}
                </DropdownMenu>
            </div>

            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon">
                        <Sun className="h-[1.2rem] w-[1.2rem] rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
                        <Moon className="absolute h-[1.2rem] w-[1.2rem] rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
                        <span className="sr-only">Toggle theme</span>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                    <DropdownMenuItem onClick={() => setTheme("light")}>
                        Light
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setTheme("dark")}>
                        Dark
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setTheme("system")}>
                        System
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>

            <Button variant="ghost" size="icon" className="relative">
                <Bell className="h-5 w-5" />
                <span className="absolute right-2 top-2 h-2 w-2 rounded-full bg-red-600"></span>
            </Button>

            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                        <Avatar className="h-8 w-8">
                            <AvatarImage src="/avatars/01.png" alt={user?.firstName || "User"} />
                            <AvatarFallback>{user?.firstName?.[0] || "U"}</AvatarFallback>
                        </Avatar>
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56" align="end" forceMount>
                    <DropdownMenuLabel className="font-normal">
                        <div className="flex flex-col space-y-1">
                            <p className="text-sm font-medium leading-none">{user?.firstName} {user?.lastName}</p>
                            <p className="text-xs leading-none text-muted-foreground">
                                {user?.email}
                            </p>
                        </div>
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                        <Link href="/dashboard/profile" className="cursor-pointer">Profile</Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                        <Link href="/dashboard/settings" className="cursor-pointer">Settings</Link>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={logout} className="text-red-600 focus:text-red-600">
                        <LogOut className="mr-2 h-4 w-4" />
                        <span>Log out</span>
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>
        </header >
    );
}

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
    const [isCollapsed, setIsCollapsed] = useState(false);

    return (
        <div className="min-h-screen bg-muted/40">
            <Sidebar isCollapsed={isCollapsed} toggleCollapse={() => setIsCollapsed(!isCollapsed)} />
            <Header isSidebarCollapsed={isCollapsed} onMobileMenuOpen={() => { }} />

            <main
                className={cn(
                    "transition-all duration-300 min-h-[calc(100vh-4rem)] pt-24 px-6 pb-6",
                    isCollapsed ? "ml-[70px]" : "ml-64",
                    "max-md:ml-0"
                )}
            >
                {children}
            </main>
        </div>
    );
}
