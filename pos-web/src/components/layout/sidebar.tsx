"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
    Collapsible,
    CollapsibleContent,
    CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { ChevronLeft, ChevronRight, ChevronDown } from "lucide-react";
import { navItems, getFilteredNavItems } from "./nav-items";
import { useAuth } from "@/hooks/use-auth";

export function Sidebar({ isCollapsed, toggleCollapse }: { isCollapsed: boolean; toggleCollapse: () => void }) {
    const pathname = usePathname();
    const { user } = useAuth();
    
    // Fall back to all if user logic is weird, but ideally filter strictly.
    // user.permissions should exist. If user.role === "Admin", you might optionally bypass, 
    // but we seed Admin with all permissions anyway.
    const filteredNavItems = getFilteredNavItems(navItems, user?.permissions);

    return (
        <aside
            className={cn(
                "fixed left-0 top-0 z-40 h-screen border-r bg-white dark:bg-gray-950 transition-all duration-300",
                isCollapsed ? "w-[70px]" : "w-64"
            )}
        >
            <div className="flex h-16 items-center border-b px-4 justify-between">
                <Link href="/dashboard" className="flex items-center gap-2 font-semibold">
                    <div className="h-8 w-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground font-bold">
                        P
                    </div>
                    {!isCollapsed && <span className="text-lg">POS System</span>}
                </Link>
                <Button variant="ghost" size="icon" onClick={toggleCollapse} className="hidden md:flex">
                    {isCollapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
                </Button>
            </div>

            <div className="flex flex-col gap-2 py-4">
                <nav className="grid gap-1 px-2">
                    {filteredNavItems.map((item, index) => {
                        const Icon = item.icon;
                        const isActive = pathname === item.href || (item.children ? item.children.some(child => pathname.startsWith(child.href)) : item.matches && item.matches.some(m => pathname.startsWith(m)));
                        const isExpanded = item.children && item.children.some(child => pathname.startsWith(child.href));

                        if (item.children && !isCollapsed) {
                            return (
                                <Collapsible key={index} defaultOpen={isExpanded} className="group/collapsible">
                                    <CollapsibleTrigger asChild>
                                        <button
                                            className={cn(
                                                "flex w-full items-center justify-between rounded-lg px-3 py-2 text-sm font-medium transition-all hover:bg-accent hover:text-accent-foreground",
                                                isActive ? "text-primary hover:text-primary" : "text-muted-foreground"
                                            )}
                                        >
                                            <div className="flex items-center gap-3">
                                                <Icon className="h-5 w-5" />
                                                <span>{item.title}</span>
                                            </div>
                                            <ChevronDown className="h-4 w-4 transition-transform group-data-[state=open]/collapsible:rotate-180" />
                                        </button>
                                    </CollapsibleTrigger>
                                    <CollapsibleContent>
                                        <div className="ml-4 mt-1 space-y-1 border-l pl-2">
                                            {item.children.map((child, childIndex) => {
                                                const ChildIcon = child.icon;
                                                const isChildActive = pathname === child.href;
                                                return (
                                                    <Link
                                                        key={childIndex}
                                                        href={child.href}
                                                        className={cn(
                                                            "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all hover:bg-accent hover:text-accent-foreground",
                                                            isChildActive ? "bg-primary/10 text-primary" : "text-muted-foreground"
                                                        )}
                                                    >
                                                        <ChildIcon className="h-4 w-4" />
                                                        <span>{child.title}</span>
                                                    </Link>
                                                )
                                            })}
                                        </div>
                                    </CollapsibleContent>
                                </Collapsible>
                            )
                        }

                        return (
                            <Link
                                key={index}
                                href={item.href}
                                className={cn(
                                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all hover:bg-accent hover:text-accent-foreground",
                                    isActive ? "bg-primary text-primary-foreground hover:bg-primary/90 hover:text-primary-foreground" : "text-muted-foreground",
                                    isCollapsed && "justify-center px-2"
                                )}
                                title={isCollapsed ? item.title : undefined}
                            >
                                <Icon className="h-5 w-5" />
                                {!isCollapsed && <span>{item.title}</span>}
                            </Link>
                        );
                    })}
                </nav>
            </div>
        </aside>
    );
}
