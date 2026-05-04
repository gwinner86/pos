"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/hooks/use-auth";
import api from "@/lib/api";
import { Loader2, MapPin, ArrowLeft } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Location } from "@/types/auth";
import { ApiResponse } from "@/types/api";
import { toast } from "sonner";
import { SelectionHeader } from "@/components/layout/selection-header";

export default function SelectLocationPage() {
    const [locations, setLocations] = useState<Location[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const { user, selectedCompany, setSelectedLocation } = useAuth();
    const router = useRouter();

    useEffect(() => {
        if (!user) return; // Wait for user to be loaded

        if (!selectedCompany) {
            router.push("/select-company");
            return;
        }

        const fetchLocations = async () => {
            try {
                const response = await api.get<{ data: Location[] }>(`/api/Users/me/companies/${selectedCompany.companyId}/locations`);
                const fetchedLocations = response.data.data;
                setLocations(fetchedLocations);

                if (fetchedLocations.length === 0) {
                    toast.error("No locations found for this company.");
                    // Allow going back
                } else if (fetchedLocations.length === 1) {
                    handleSelectLocation(fetchedLocations[0]);
                }
            } catch (error) {
                console.error("Failed to fetch locations", error);
                toast.error("Failed to load locations.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchLocations();
    }, [user, selectedCompany, router]);

    const handleSelectLocation = (location: Location) => {
        setSelectedLocation(location);
        router.push("/dashboard");
    };

    const handleBack = () => {
        router.push("/select-company");
    };

    if (isLoading) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (!selectedCompany) return null; // Should redirect

    if (locations.length === 1) {
        return (
            <div className="flex h-screen w-full items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <span className="ml-2">Redirecting to dashboard...</span>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-100 dark:bg-gray-900">
            <SelectionHeader />
            <div className="flex flex-col items-center justify-center min-h-[calc(100vh-4rem)] pt-16 p-4">
                <Card className="w-full max-w-md shadow-lg border-muted/40">
                    <CardHeader className="space-y-1 text-center">
                        <CardTitle className="text-2xl font-bold tracking-tight">
                            Select Location
                        </CardTitle>
                        <CardDescription>
                            Choose a location/branch for {selectedCompany.companyName}
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        {locations.length === 0 ? (
                            <p className="text-center text-muted-foreground">No locations found.</p>
                        ) : (
                            locations.map((location) => (
                                <Button
                                    key={location.id}
                                    variant="outline"
                                    className="w-full h-auto p-4 justify-start text-left flex items-center gap-4 hover:bg-accent/50"
                                    onClick={() => handleSelectLocation(location)}
                                >
                                    <div className="bg-primary/10 p-2 rounded-full">
                                        <MapPin className="h-6 w-6 text-primary" />
                                    </div>
                                    <div className="flex flex-col">
                                        <span className="font-semibold">{location.locationName}</span>
                                        <span className="text-xs text-muted-foreground">{location.locationType}</span>
                                    </div>
                                </Button>
                            ))
                        )}
                    </CardContent>
                    <CardFooter>
                        <Button variant="ghost" className="w-full" onClick={handleBack}>
                            <ArrowLeft className="mr-2 h-4 w-4" />
                            Back to Companies
                        </Button>
                    </CardFooter>
                </Card>
            </div>
        </div>
    );
}
