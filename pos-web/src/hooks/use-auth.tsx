"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { AuthState, User, Tenant, Company, Location } from "@/types/auth";
import { ApiResponse } from "@/types/api";
import api from "@/lib/api";

interface AuthContextType extends AuthState {
    login: (token: string, user: User, redirectPath?: string) => void;
    logout: () => void;
    verifyTenant: (name: string) => Promise<Tenant>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [state, setState] = useState<AuthState>({
        user: null,
        tenant: null,
        selectedCompany: null,
        selectedLocation: null,
        isAuthenticated: false,
        isLoading: true,
        setSelectedCompany: () => { },
        setSelectedLocation: () => { }
    });
    const router = useRouter();

    const logout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        localStorage.removeItem("company");
        localStorage.removeItem("location");
        setState(prev => ({
            ...prev,
            user: null,
            selectedCompany: null,
            selectedLocation: null,
            isAuthenticated: false,
            isLoading: false,
        }));
        router.push("/login");
    };

    useEffect(() => {
        const initializeAuth = async () => {
            const token = localStorage.getItem("token");
            const storedUser = localStorage.getItem("user");
            const storedTenant = localStorage.getItem("tenant");

            let tenant: Tenant | null = null;
            if (storedTenant) {
                try {
                    tenant = JSON.parse(storedTenant);
                } catch (e) {
                    console.error("Failed to parse stored tenant", e);
                    localStorage.removeItem("tenant");
                }
            }

            const storedCompany = localStorage.getItem("company");
            let company: Company | null = null;
            if (storedCompany) {
                try {
                    company = JSON.parse(storedCompany);
                } catch (e) {
                    console.error("Failed to parse stored company", e);
                    localStorage.removeItem("company");
                }
            }

            const storedLocation = localStorage.getItem("location");
            let location: Location | null = null;
            if (storedLocation) {
                try {
                    location = JSON.parse(storedLocation);
                } catch (e) {
                    console.error("Failed to parse stored location", e);
                    localStorage.removeItem("location");
                }
            }

            if (token && storedUser) {
                try {
                    const user: User = JSON.parse(storedUser);
                    setState(prev => ({
                        ...prev,
                        user,
                        tenant,
                        selectedCompany: company,
                        selectedLocation: location,
                        isAuthenticated: true,
                        isLoading: false,
                    }));
                } catch (error) {
                    console.error("Auth initialization failed:", error);
                    logout();
                }
            } else {
                setState(prev => ({
                    ...prev,
                    tenant,
                    selectedCompany: null,
                    selectedLocation: null,
                    isLoading: false
                }));
            }
        };

        initializeAuth();
    }, []);

    const login = (token: string, user: User, redirectPath: string = "/select-company") => {
        localStorage.setItem("token", token);
        localStorage.setItem("user", JSON.stringify(user));
        setState(prev => ({
            ...prev,
            user,
            isAuthenticated: true,
            isLoading: false,
        }));
        if (redirectPath) {
            router.push(redirectPath);
        }
    };

    const verifyTenant = async (name: string): Promise<Tenant> => {
        try {
            const response = await api.get<ApiResponse<Tenant>>(`/api/Tenants/by-name/${name}`);
            const tenant = response.data.data;

            localStorage.setItem("tenant", JSON.stringify(tenant));
            setState(prev => ({ ...prev, tenant }));

            return tenant;
        } catch (error) {
            throw error;
        }
    }

    const setSelectedCompany = (company: Company) => {
        localStorage.setItem("company", JSON.stringify(company));
        setState(prev => ({ ...prev, selectedCompany: company }));
    };

    const setSelectedLocation = (location: Location) => {
        localStorage.setItem("location", JSON.stringify(location));
        setState(prev => ({ ...prev, selectedLocation: location }));
    };

    return (
        <AuthContext.Provider value={{ ...state, login, logout, verifyTenant, setSelectedCompany, setSelectedLocation }}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
}
