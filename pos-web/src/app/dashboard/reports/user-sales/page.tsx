"use client";

import { useEffect, useState } from "react";
import { format } from "date-fns";
import { Calendar as CalendarIcon, Loader2, Users, Download, Printer } from "lucide-react";
import * as React from "react";
import { DateRange } from "react-day-picker";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { formatCurrency } from "@/lib/utils";
import { useAuth } from "@/hooks/use-auth";
import { salesService } from "@/services/sales-service";
import { settingsService, UserResponse } from "@/services/settings-service";
import { Sale } from "@/types/sales";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export default function UserTransactionsPage() {
    const { selectedLocation } = useAuth();
    const [sales, setSales] = useState<Sale[]>([]);
    const [users, setUsers] = useState<UserResponse[]>([]);
    const [loading, setLoading] = useState(true);

    const [selectedUserId, setSelectedUserId] = useState<string>("all");
    const [date, setDate] = useState<DateRange | undefined>({
        from: new Date(new Date().setHours(0, 0, 0, 0)),
        to: new Date(new Date().setHours(23, 59, 59, 999)),
    });

    const currencySymbol = selectedLocation?.currencySymbol || "$";

    const fetchDropdownData = async () => {
        try {
            const usersData = await settingsService.getUsers(false);
            setUsers(usersData);
        } catch (error) {
            toast.error("Failed to load users for filter.");
        }
    };

    const fetchSales = async () => {
        setLoading(true);
        try {
            const fromStr = date?.from ? date.from.toISOString() : undefined;
            const toStr = date?.to ? date.to.toISOString() : undefined;
            const userFilter = selectedUserId !== "all" ? selectedUserId : undefined;
            
            const data = await salesService.getSales(undefined, userFilter, fromStr, toStr);
            setSales(data);
        } catch (error) {
            toast.error("Failed to load transactions.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchDropdownData();
    }, []);

    useEffect(() => {
        fetchSales();
    }, [selectedUserId, date]);

    const totalTransactions = sales.length;
    const totalRevenue = sales.reduce((sum, sale) => sum + sale.totalAmount, 0);

    const handleExportExcel = () => {
        if (sales.length === 0) {
            toast.error("No transactions to export.");
            return;
        }
        
        const headers = ["Sale No.", "Date", "Cashier", "Customer", "Payment Method", "Status", "Total Amount"];
        const rows = sales.map(s => [
            s.saleNumber,
            format(new Date(s.saleDate), "PPp").replace(/,/g, ''), // remove commas to prevent csv breaking
            s.createdByUserName || "Unknown",
            `"${s.customerName}"`,
            s.paymentMethod,
            s.status,
            s.totalAmount
        ]);
        
        let csvContent = headers.join(",") + "\n" + rows.map(e => e.join(",")).join("\n");
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement("a");
        const url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", `user_transactions_${format(new Date(), 'yyyyMMdd')}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const handlePrint = () => {
        window.print();
    };

    return (
        <div className="space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">User Transactions Report</h2>
                    <p className="text-muted-foreground print:hidden">
                        Audit and review sales transactions grouped by specific users or cashiers.
                    </p>
                </div>
                <div className="flex gap-2 print:hidden">
                    <Button variant="outline" onClick={handlePrint}>
                        <Printer className="mr-2 h-4 w-4" />
                        Print
                    </Button>
                    <Button variant="default" onClick={handleExportExcel}>
                        <Download className="mr-2 h-4 w-4" />
                        Export
                    </Button>
                </div>
            </div>

            <div className="flex flex-col md:flex-row gap-4 items-center justify-between print:hidden">
                <div className="flex items-center gap-4 w-full md:w-auto">
                    <Select value={selectedUserId} onValueChange={setSelectedUserId}>
                        <SelectTrigger className="w-[200px]">
                            <SelectValue placeholder="Select User" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Users</SelectItem>
                            {users.map(u => (
                                <SelectItem key={u.userId} value={u.userId}>
                                    {u.firstName} {u.lastName}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>

                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                variant={"outline"}
                                className={cn(
                                    "w-[260px] justify-start text-left font-normal",
                                    !date && "text-muted-foreground"
                                )}
                            >
                                <CalendarIcon className="mr-2 h-4 w-4" />
                                {date?.from ? (
                                    date.to ? (
                                        <>
                                            {format(date.from, "LLL dd, y")} -{" "}
                                            {format(date.to, "LLL dd, y")}
                                        </>
                                    ) : (
                                        format(date.from, "LLL dd, y")
                                    )
                                ) : (
                                    <span>Pick a date range</span>
                                )}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="start">
                            <Calendar
                                initialFocus
                                mode="range"
                                defaultMonth={date?.from}
                                selected={date}
                                onSelect={setDate}
                                numberOfMonths={2}
                            />
                        </PopoverContent>
                    </Popover>
                </div>

                <div className="flex gap-4">
                    <Card className="px-4 py-2 border-primary/20 bg-primary/5">
                        <div className="text-sm text-muted-foreground">Total Transactions</div>
                        <div className="text-2xl font-bold">{totalTransactions}</div>
                    </Card>
                    <Card className="px-4 py-2 border-primary/20 bg-primary/5">
                        <div className="text-sm text-muted-foreground">Total Revenue</div>
                        <div className="text-2xl font-bold text-green-600 dark:text-green-400">
                            {formatCurrency(totalRevenue, currencySymbol)}
                        </div>
                    </Card>
                </div>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Transactions Log</CardTitle>
                </CardHeader>
                <CardContent>
                    {loading ? (
                        <div className="flex justify-center p-8">
                            <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Sale No.</TableHead>
                                    <TableHead>Date</TableHead>
                                    <TableHead>Cashier</TableHead>
                                    <TableHead>Customer</TableHead>
                                    <TableHead>Payment</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead className="text-right">Total Amount</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {sales.length === 0 ? (
                                    <TableRow>
                                        <TableCell colSpan={7} className="text-center py-8 text-muted-foreground">
                                            No transactions found for this period/user.
                                        </TableCell>
                                    </TableRow>
                                ) : (
                                    sales.map((sale) => (
                                        <TableRow key={sale.id}>
                                            <TableCell className="font-medium">{sale.saleNumber}</TableCell>
                                            <TableCell>{format(new Date(sale.saleDate), "PPp")}</TableCell>
                                            <TableCell>
                                                <div className="flex items-center gap-2">
                                                    <Users className="h-4 w-4 text-muted-foreground" />
                                                    {sale.createdByUserName || "Unknown"}
                                                </div>
                                            </TableCell>
                                            <TableCell>{sale.customerName}</TableCell>
                                            <TableCell>{sale.paymentMethod}</TableCell>
                                            <TableCell>
                                                <Badge variant={sale.status === "Completed" ? "default" : "secondary"}>
                                                    {sale.status}
                                                </Badge>
                                            </TableCell>
                                            <TableCell className="text-right font-medium">
                                                {formatCurrency(sale.totalAmount, currencySymbol)}
                                            </TableCell>
                                        </TableRow>
                                    ))
                                )}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
