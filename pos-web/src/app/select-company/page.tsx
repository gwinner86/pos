"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import api from "@/lib/api";
import { Loader2, Building2 } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

import { Company, LoginResponse, User } from "@/types/auth";
import { ApiResponse } from "@/types/api";
import { toast } from "sonner";
import { SelectionHeader } from "@/components/layout/selection-header";

export default function SelectCompanyPage() {
    const [companies, setCompanies] = useState<Company[]>([]);
    const [isFetchingCompanies, setIsFetchingCompanies] = useState(false); // Start false, wait for user
    const { setSelectedCompany, user, login, isLoading: authLoading } = useAuth();
    const router = useRouter();

    useEffect(() => {
        // 1. Wait for Auth to initialize
        if (authLoading) return;

        // 2. If no user after auth init, redirect
        if (!user) {
            router.push("/login");
            return;
        }

        // 3. User authenticated, fetch companies
        const fetchCompanies = async () => {
            try {
                setIsFetchingCompanies(true);
                let response;

                // Case 1: Tenant User
                if (user.tenantId) {
                    console.log("Fetching companies for tenant user:", user.tenantId);
                    response = await api.get<{ data: Company[] }>(`/api/Companies/tenant/${user.tenantId}`);
                }
                // Case 2: Admin (No Tenant)
                else if (user.role === "Admin") {
                    console.log("Fetching all companies for Admin");
                    response = await api.get<{ data: Company[] }>("/api/Companies");
                }
                // Fallback
                else {
                    console.log("Fallback: Fetching user companies");
                    response = await api.get<{ data: Company[] }>("/api/Users/me/companies");
                }

                setCompanies(response.data.data);
            } catch (error) {
                console.error("Failed to fetch companies:", error);
                toast.error("Failed to load companies.");
            } finally {
                setIsFetchingCompanies(false);
            }
        };

        fetchCompanies();
    }, [user, authLoading, router]);



    const handleSelectCompany = async (company: Company) => {
        try {
            // Call API to switch context and get new token
            const response = await api.post<ApiResponse<LoginResponse>>(`/api/Auth/switch-company/${company.companyId}`);

            const loginData = response.data.data;

            if (loginData.token && user) {
                const authUser: User = {
                    id: loginData.userId,
                    email: loginData.email,
                    firstName: loginData.firstName,
                    lastName: loginData.lastName,
                    role: loginData.role,
                    tenantId: loginData.tenantId,
                    companyId: loginData.companyId,
                    permissions: loginData.permissions,
                    requiresPasswordChange: loginData.requiresPasswordChange
                };

                login(loginData.token, authUser); // Update token and user

                setSelectedCompany(company);
                router.push("/select-location");
            }
        } catch (error) {
            console.error("Failed to switch company", error);
            toast.error("Failed to switch company context.");
        }
    };

    if (authLoading || isFetchingCompanies) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (companies.length === 0) {
        return (
            <div className="flex h-screen items-center justify-center">
                <p>No companies assigned to you. Please contact your administrator.</p>
            </div>
        );
    }

    // Only render selection UI if multiple companies (or if auto-select didn't happen fast enough which might flicker, but logic above handles 1)
    // Actually, it's better to show loading until decision is made. Logic above sets companies then IsLoading false.
    // If 1 company, handleSelectCompany calls router.push. We might still render for a split second.
    // To avoid flicker, we can check length in return.



    return (
        <div className="min-h-screen bg-gray-100 dark:bg-gray-900">
            <SelectionHeader />
            <div className="flex flex-col items-center pt-24 p-8">
                <div className="w-full max-w-5xl space-y-8">
                    <div className="text-center space-y-2">
                        <h1 className="text-3xl font-bold tracking-tight">Select Company</h1>
                        <p className="text-muted-foreground">Choose a company to access your dashboard</p>
                    </div>

                    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 place-items-center md:place-items-stretch">
                        {companies.map((company) => (
                            <Card
                                key={company.companyId}
                                className="cursor-pointer hover:shadow-lg transition-all hover:border-primary/50 group w-full max-w-sm"
                                onClick={() => handleSelectCompany(company)}
                            >
                                <CardHeader className="flex flex-row items-center gap-4 space-y-0">
                                    <div className="bg-primary/10 p-3 rounded-full group-hover:bg-primary/20 transition-colors">
                                        <Building2 className="h-6 w-6 text-primary" />
                                    </div>
                                    <div className="space-y-1">
                                        <CardTitle className="text-lg">{company.companyName}</CardTitle>
                                        {/* <CardDescription>ID: {company.companyId.substring(0, 8)}</CardDescription> */}
                                    </div>
                                </CardHeader>
                                <CardContent>
                                    <div className="flex items-center text-sm text-muted-foreground">
                                        <span className="flex h-2 w-2 rounded-full bg-green-500 mr-2" />
                                        Active
                                    </div>
                                </CardContent>
                            </Card>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}
