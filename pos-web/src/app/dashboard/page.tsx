"use client";

import { useAuth } from "@/hooks/use-auth";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DollarSign, CreditCard, Activity, Users, Calendar } from "lucide-react";
import { formatCurrency } from "@/lib/utils";
import { OverviewChart } from "@/app/dashboard/components/overview-chart";
import { useEffect, useState } from "react";
import { dashboardService, DashboardStats } from "@/services/dashboard-service";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ProductPerformanceWidget } from "@/app/dashboard/components/product-performance-widget";
import { PaymentPieChart } from "@/app/dashboard/components/payment-pie-chart";

export default function DashboardPage() {
    const { user } = useAuth();
    const [stats, setStats] = useState<DashboardStats | null>(null);
    const [loading, setLoading] = useState(true);
    const [dateFilter, setDateFilter] = useState("today");

    useEffect(() => {
        const loadStats = async () => {
            setLoading(true);
            try {
                const data = await dashboardService.getStats(dateFilter === "all_time" ? undefined : dateFilter);
                setStats(data);
            } catch (error) {
                console.error("Failed to load dashboard stats", error);
            } finally {
                setLoading(false);
            }
        };
        loadStats();
    }, [dateFilter]);

    if (loading) {
        return <div className="p-8">Loading dashboard...</div>;
    }

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
                    <div className="flex items-center space-x-2 mt-1">
                        <span className="text-sm text-muted-foreground">Welcome back, {user?.firstName}!</span>
                    </div>
                </div>

                <div className="flex items-center space-x-2">
                    <Select value={dateFilter} onValueChange={setDateFilter}>
                        <SelectTrigger className="w-[180px] bg-white">
                            <Calendar className="mr-2 h-4 w-4 text-muted-foreground" />
                            <SelectValue placeholder="Select Date Range" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="today">Today</SelectItem>
                            <SelectItem value="yesterday">Yesterday</SelectItem>
                            <SelectItem value="last_week">Last Week</SelectItem>
                            <SelectItem value="last_month">Last Month</SelectItem>
                            <SelectItem value="all_time">All Time</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Total Revenue</CardTitle>
                        <DollarSign className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{formatCurrency(stats?.totalRevenue || 0)}</div>
                        <p className="text-xs text-muted-foreground">{dateFilter === "all_time" ? "Lifetime Revenue" : "Revenue"}</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Sales</CardTitle>
                        <CreditCard className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">+{stats?.salesCount || 0}</div>
                        <p className="text-xs text-muted-foreground">{dateFilter === "all_time" ? "Total Transactions" : "Transactions"}</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Active Products</CardTitle>
                        <Activity className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{stats?.activeProductsCount || 0}</div>
                        <p className="text-xs text-muted-foreground">Items in Catalog</p>
                    </CardContent>
                </Card>
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                        <CardTitle className="text-sm font-medium">Active Customers</CardTitle>
                        <Users className="h-4 w-4 text-muted-foreground" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-2xl font-bold">{stats?.activeCustomersCount || 0}</div>
                        <p className="text-xs text-muted-foreground">Registered Customers</p>
                    </CardContent>
                </Card>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
                <Card className="col-span-4">
                    <CardHeader>
                        <CardTitle>Overview</CardTitle>
                    </CardHeader>
                    <CardContent className="pl-2">
                        <OverviewChart data={stats?.salesTrend || []} />
                    </CardContent>
                </Card>
                <Card className="col-span-3">
                    <CardHeader>
                        <CardTitle>Recent Sales</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-8">
                            {stats?.recentSales?.map((sale, i) => (
                                <div key={i} className="flex items-center">
                                    <div className="ml-4 space-y-1">
                                        <p className="text-sm font-medium leading-none">{sale.customerName || "Walk-in Customer"}</p>
                                        <p className="text-xs text-muted-foreground">{sale.saleNumber}</p>
                                    </div>
                                    <div className="ml-auto font-medium">+{formatCurrency(sale.totalAmount)}</div>
                                </div>
                            ))}
                            {(!stats?.recentSales || stats.recentSales.length === 0) && (
                                <p className="text-sm text-muted-foreground text-center py-4">No recent sales.</p>
                            )}
                        </div>
                    </CardContent>
                </Card>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                <ProductPerformanceWidget
                    title="Fast Moving Products"
                    data={stats?.fastMovingProducts || []}
                    type="fast"
                />
                <ProductPerformanceWidget
                    title="Slow Moving Products"
                    data={stats?.slowMovingProducts || []}
                    type="slow"
                />
                <PaymentPieChart
                    data={stats?.paymentMethodStats || []}
                />
            </div>
        </div>
    );
}
