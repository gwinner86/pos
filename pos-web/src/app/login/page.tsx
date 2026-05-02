"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Eye, EyeOff, Loader2, Lock, Mail } from "lucide-react";
import { toast } from "sonner";

import { useAuth } from "@/hooks/use-auth";
import api from "@/lib/api";

import { Button } from "@/components/ui/button";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";

const loginSchema = z.object({
    email: z.string().email("Please enter a valid email address"),
    password: z.string().min(1, "Password is required"),
    tenantName: z.string().optional(),
    rememberMe: z.boolean().optional(),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export default function LoginPage() {
    const { login, verifyTenant, tenant } = useAuth();
    const [isLoading, setIsLoading] = useState(false);
    const [isVerifyingTenant, setIsVerifyingTenant] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const form = useForm<LoginFormValues>({
        resolver: zodResolver(loginSchema),
        defaultValues: {
            email: "",
            password: "",
            tenantName: tenant?.tenantName || "",
            rememberMe: false,
        },
    });

    // Effect to set default tenant name if loaded from verification
    const [isTenantVerified, setIsTenantVerified] = useState(!!tenant);

    useEffect(() => {
        if (tenant) {
            form.setValue("tenantName", tenant.tenantName);
            setIsTenantVerified(true);
        }
    }, [tenant, form]);


    const handleVerifyTenant = async () => {
        const name = form.getValues("tenantName");
        if (!name) {
            toast.error("Please enter a tenant name to verify.");
            return;
        }

        setIsVerifyingTenant(true);
        try {
            await verifyTenant(name);
            setIsTenantVerified(true);
            toast.success("Tenant verified successfully.");
        } catch {
            toast.error("Tenant not found.");
            setIsTenantVerified(false);
        } finally {
            setIsVerifyingTenant(false);
        }
    };

    const resetTenant = () => {
        setIsTenantVerified(false);
        form.setValue("tenantName", "");
        // Ideally we might want to clear it from auth state/storage too if they explicitly reset, 
        // but for now we just allow re-entry.
    };

    async function onSubmit(data: LoginFormValues) {
        setIsLoading(true);
        try {
            const response = await api.post("/api/Auth/login", {
                email: data.email,
                password: data.password,
                tenantName: data.tenantName || null,
            });

            // response.data is ApiResponse<LoginResponse>
            const apiResponse = response.data;
            const loginData = apiResponse.data;

            // map api user to local user
            const authUser = {
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

            const redirectPath = authUser.requiresPasswordChange ? "/change-password" : "/select-company";

            login(loginData.token, authUser, redirectPath);
            toast.success("Welcome back!");
        } catch (error) {
            console.error("Login Error:", error);
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const message = (error as any).response?.data?.message || "Invalid email or password.";
            toast.error(message);
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <div className="flex min-h-screen items-center justify-center bg-gray-100 dark:bg-gray-900 p-4">
            <Card className="w-full max-w-md shadow-lg border-muted/40">
                <CardHeader className="space-y-1 text-center">
                    <CardTitle className="text-2xl font-bold tracking-tight">
                        Welcome back
                    </CardTitle>
                    <CardDescription>
                        Enter your credentials to access your account
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                            <FormField
                                control={form.control}
                                name="tenantName"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Tenant Name</FormLabel>
                                        <div className="flex gap-2">
                                            <FormControl>
                                                <Input
                                                    placeholder="Enter tenant name"
                                                    {...field}
                                                    disabled={isTenantVerified}
                                                    className={isTenantVerified ? "bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800" : ""}
                                                />
                                            </FormControl>

                                            {isTenantVerified ? (
                                                <Button
                                                    type="button"
                                                    variant="ghost"
                                                    onClick={resetTenant}
                                                    className="text-muted-foreground hover:text-foreground"
                                                >
                                                    Change
                                                </Button>
                                            ) : (
                                                <Button
                                                    type="button"
                                                    variant="secondary"
                                                    onClick={handleVerifyTenant}
                                                    disabled={isVerifyingTenant}
                                                >
                                                    {isVerifyingTenant ? <Loader2 className="h-4 w-4 animate-spin" /> : "Verify"}
                                                </Button>
                                            )}
                                        </div>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="email"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Email</FormLabel>
                                        <FormControl>
                                            <div className="relative">
                                                <Mail className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
                                                <Input
                                                    placeholder="name@example.com"
                                                    className="pl-9"
                                                    {...field}
                                                />
                                            </div>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="password"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Password</FormLabel>
                                        <FormControl>
                                            <div className="relative">
                                                <Lock className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
                                                <Input
                                                    type={showPassword ? "text" : "password"}
                                                    placeholder="Enter your password"
                                                    className="pl-9 pr-9"
                                                    {...field}
                                                />
                                                <Button
                                                    type="button"
                                                    variant="ghost"
                                                    size="sm"
                                                    className="absolute right-1 top-1 h-7 w-7 p-0 hover:bg-transparent"
                                                    onClick={() => setShowPassword(!showPassword)}
                                                >
                                                    {showPassword ? (
                                                        <EyeOff className="h-4 w-4 text-muted-foreground" />
                                                    ) : (
                                                        <Eye className="h-4 w-4 text-muted-foreground" />
                                                    )}
                                                    <span className="sr-only">Toggle password visibility</span>
                                                </Button>
                                            </div>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <div className="flex items-center justify-between">
                                <FormField
                                    control={form.control}
                                    name="rememberMe"
                                    render={({ field }) => (
                                        <FormItem className="flex flex-row items-start space-x-2 space-y-0">
                                            <FormControl>
                                                <Checkbox
                                                    checked={field.value}
                                                    onCheckedChange={field.onChange}
                                                />
                                            </FormControl>
                                            <div className="space-y-1 leading-none">
                                                <FormLabel className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                                                    Remember me
                                                </FormLabel>
                                            </div>
                                        </FormItem>
                                    )}
                                />
                                <Button variant="link" size="sm" className="px-0 font-normal" type="button">
                                    Forgot password?
                                </Button>
                            </div>

                            <Button className="w-full" type="submit" disabled={isLoading}>
                                {isLoading ? (
                                    <>
                                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                        Signing in...
                                    </>
                                ) : (
                                    "Sign In"
                                )}
                            </Button>
                        </form>
                    </Form>
                </CardContent>
                <CardFooter className="flex justify-center text-sm text-muted-foreground">
                    Don&apos;t have an account? Contact your administrator.
                </CardFooter>
            </Card>
        </div>
    );
}
