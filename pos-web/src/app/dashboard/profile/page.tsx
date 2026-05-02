"use client";

import { useAuth } from "@/hooks/use-auth";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Mail, Briefcase, Building2, MapPin, Settings, ShieldCheck, User as UserIcon } from "lucide-react";
import Link from "next/link";
import { format } from "date-fns";

export default function ProfilePage() {
    const { user, selectedCompany, selectedLocation } = useAuth();

    if (!user) {
        return null; // Or a skeleton loader
    }

    const initials = `${user.firstName?.[0] || ""}${user.lastName?.[0] || ""}`;

    return (
        <div className="max-w-4xl mx-auto space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Your Profile</h2>
                <p className="text-muted-foreground">
                    Manage your personal account details and access preferences.
                </p>
            </div>

            <div className="grid gap-6 md:grid-cols-[1fr_300px]">
                {/* Main Profile Info */}
                <Card className="border-t-4 border-t-primary shadow-md">
                    <CardHeader className="pb-4">
                        <div className="flex items-center gap-6">
                            <Avatar className="h-24 w-24 border-4 border-background shadow-sm">
                                <AvatarImage src="/avatars/01.png" alt={user.firstName} />
                                <AvatarFallback className="text-3xl bg-primary/10 text-primary font-light">
                                    {initials}
                                </AvatarFallback>
                            </Avatar>
                            <div className="space-y-1">
                                <h3 className="text-2xl font-bold">{user.firstName} {user.lastName}</h3>
                                <div className="flex items-center gap-2 text-muted-foreground mr-2">
                                    <Mail className="h-4 w-4" />
                                    <span>{user.email}</span>
                                </div>
                                <Badge variant="secondary" className="mt-2 font-medium tracking-wide">
                                    {user.role}
                                </Badge>
                            </div>
                        </div>
                    </CardHeader>
                    <CardContent className="space-y-6 mt-4">
                        <div className="grid gap-4 sm:grid-cols-2">
                            <div className="space-y-1.5 p-4 rounded-lg bg-muted/50 border border-muted">
                                <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground mb-1">
                                    <Building2 className="h-4 w-4" />
                                    Active Company
                                </div>
                                <p className="font-semibold">{selectedCompany?.companyName || "N/A"}</p>
                            </div>

                            <div className="space-y-1.5 p-4 rounded-lg bg-muted/50 border border-muted">
                                <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground mb-1">
                                    <MapPin className="h-4 w-4" />
                                    Active Branch / Location
                                </div>
                                <p className="font-semibold">{selectedLocation?.locationName || "N/A"}</p>
                            </div>
                        </div>

                        <div className="flex justify-start gap-4 pt-4 border-t">
                            <Button asChild>
                                <Link href="/dashboard/settings">
                                    <Settings className="mr-2 h-4 w-4" />
                                    Account Settings
                                </Link>
                            </Button>
                        </div>
                    </CardContent>
                </Card>

                {/* Sidebar Info */}
                <div className="space-y-6">
                    <Card className="shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-lg flex items-center gap-2">
                                <ShieldCheck className="h-5 w-5 text-primary" />
                                Security Status
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="space-y-1">
                                <p className="text-sm font-medium">Password Reset Requirement</p>
                                <div className="text-sm text-muted-foreground">
                                    {user.requiresPasswordChange ? (
                                        <span className="text-amber-600 flex items-center gap-1">
                                            <div className="h-2 w-2 rounded-full bg-amber-600" /> Action Required On Next Login
                                        </span>
                                    ) : (
                                        <span className="text-emerald-600 flex items-center gap-1">
                                            <div className="h-2 w-2 rounded-full bg-emerald-600" /> Up to date
                                        </span>
                                    )}
                                </div>
                            </div>
                            <div className="space-y-1">
                                <p className="text-sm font-medium">Session Active</p>
                                <p className="text-sm text-muted-foreground">Currently logged in</p>
                            </div>
                        </CardContent>
                    </Card>

                    <Card className="shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-lg flex items-center gap-2">
                                <UserIcon className="h-5 w-5 text-primary" />
                                Account Details
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="space-y-1">
                                <p className="text-sm font-medium">User ID</p>
                                <p className="text-xs font-mono text-muted-foreground break-all bg-muted p-2 rounded border">
                                    {user.id}
                                </p>
                            </div>
                            <div className="space-y-1">
                                <p className="text-sm font-medium">Permissions Group</p>
                                <p className="text-sm text-muted-foreground flex items-center gap-1">
                                    <Briefcase className="h-3 w-3" />
                                    {user.permissions?.length || 0} active policies
                                </p>
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}
