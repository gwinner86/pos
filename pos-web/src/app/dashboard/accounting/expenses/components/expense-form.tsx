"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { toast } from "sonner";
import { useAuth } from "@/hooks/use-auth";
import { expenseService } from "@/services/expense-service";
import { accountingService } from "@/services/accounting-service";
import { expenseTypeService } from "@/services/expense-type-service";
import { GLAccount } from "@/types/accounting";
import { SetupExpenseType } from "@/types/settings";

const expenseSchema = z.object({
    expenseDate: z.string(),
    description: z.string().min(3, "Description is required"),
    referenceNumber: z.string().optional(),
    amount: z.string().refine((val) => !isNaN(parseFloat(val)) && parseFloat(val) > 0, {
        message: "Amount must be greater than 0",
    }),
    expenseTypeId: z.string().min(1, "Expense Type is required"),
    paymentAccountId: z.string().min(1, "Payment Account is required"),
});

export function ExpenseForm({ onSuccess }: { onSuccess: () => void }) {
    const { selectedLocation } = useAuth();
    const [accounts, setAccounts] = useState<GLAccount[]>([]);
    const [expenseTypes, setExpenseTypes] = useState<SetupExpenseType[]>([]);
    const [loading, setLoading] = useState(false);

    const form = useForm<z.infer<typeof expenseSchema>>({
        resolver: zodResolver(expenseSchema),
        defaultValues: {
            expenseDate: new Date().toISOString().split("T")[0],
            description: "",
            referenceNumber: "",
            amount: "",
            expenseTypeId: "",
            paymentAccountId: "",
        },
    });

    useEffect(() => {
        async function loadData() {
            try {
                const [accountsData, typesData] = await Promise.all([
                    accountingService.getGLAccounts(),
                    expenseTypeService.getExpenseTypes()
                ]);
                setAccounts(accountsData);
                setExpenseTypes(typesData.filter(t => t.isActive));
            } catch (error) {
                console.error("Failed to load data", error);
            }
        }
        loadData();
    }, []);

    // Filter accounts logically
    const paymentAccounts = accounts.filter(a => a.accountType === "Asset"); // Cash, Bank, etc.

    async function onSubmit(values: z.infer<typeof expenseSchema>) {
        if (!selectedLocation) {
            toast.error("No location selected");
            return;
        }

        setLoading(true);
        try {
            await expenseService.createExpense({
                locationId: selectedLocation.id,
                expenseDate: new Date(values.expenseDate).toISOString(),
                description: values.description,
                referenceNumber: values.referenceNumber,
                amount: parseFloat(values.amount),
                expenseTypeId: values.expenseTypeId,
                paymentAccountId: values.paymentAccountId,
            });
            toast.success("Expense recorded successfully");
            form.reset();
            onSuccess();
        } catch (error) {
            console.error(error);
            toast.error("Failed to record expense");
        } finally {
            setLoading(false);
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="expenseDate"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Date</FormLabel>
                                <FormControl>
                                    <Input type="date" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                    <FormField
                        control={form.control}
                        name="amount"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Amount</FormLabel>
                                <FormControl>
                                    <Input type="number" step="0.01" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="expenseTypeId"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Expense Type</FormLabel>
                                <Select onValueChange={field.onChange} defaultValue={field.value}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select Expense Type" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {expenseTypes.map((type) => (
                                            <SelectItem key={type.id} value={type.id}>
                                                {type.name}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="paymentAccountId"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Paid From (Credit)</FormLabel>
                                <Select onValueChange={field.onChange} defaultValue={field.value}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Select Payment Account" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {paymentAccounts.map((account) => (
                                            <SelectItem key={account.id} value={account.id}>
                                                {account.accountNumber} - {account.accountName}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="description"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Description</FormLabel>
                            <FormControl>
                                <Input placeholder="e.g. Office Rent for January" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="referenceNumber"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Reference # (Optional)</FormLabel>
                                <FormControl>
                                    <Input placeholder="e.g. INV-001" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <Button type="submit" className="w-full" disabled={loading}>
                    {loading ? "Recording..." : "Record Expense"}
                </Button>
            </form>
        </Form>
    );
}
