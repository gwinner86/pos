"use client";

import { useState, useEffect } from "react";
import { format } from "date-fns";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/ui/data-table";
import { columns } from "./columns";
import { expenseService } from "@/services/expense-service";
import { Expense } from "@/types/expense";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from "@/components/ui/sheet";
import { ExpenseForm } from "./components/expense-form";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";


export function ExpensesClient() {
    const [data, setData] = useState<Expense[]>([]);
    const [loading, setLoading] = useState(true);
    const [open, setOpen] = useState(false);

    const loadData = async () => {
        setLoading(true);
        try {
            // Default to current month or just fetch all for now
            const result = await expenseService.getExpenses();
            setData(result || []);
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">Expenses</h2>
                    <p className="text-muted-foreground">
                        Manage and track operational expenses.
                    </p>
                </div>
                <Sheet open={open} onOpenChange={setOpen}>
                    <SheetTrigger asChild>
                        <Button>
                            <Plus className="mr-2 h-4 w-4" /> Record Expense
                        </Button>
                    </SheetTrigger>
                    <SheetContent className="sm:max-w-xl">
                        <SheetHeader>
                            <SheetTitle>Record New Expense</SheetTitle>
                            <SheetDescription>
                                Enter details for the operational expense. This will automatically post a journal entry.
                            </SheetDescription>
                        </SheetHeader>
                        <div className="py-6">
                            <ExpenseForm onSuccess={() => {
                                setOpen(false);
                                loadData();
                            }} />
                        </div>
                    </SheetContent>
                </Sheet>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Expense History</CardTitle>
                    <CardDescription>
                        A comprehensive list of all recorded operational expenses.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {loading ? (
                        <div className="flex items-center justify-center p-8">Loading expenses...</div>
                    ) : (
                        <DataTable columns={columns} data={data} searchKey="description" />
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
