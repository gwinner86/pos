"use client";

import { useEffect, useState } from "react";
import { accountingService } from "@/services/accounting-service";
import { GLAccount } from "@/types/accounting";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Loader2 } from "lucide-react";

import { formatCurrency } from "@/lib/utils";

export function ChartOfAccounts() {
    const [accounts, setAccounts] = useState<GLAccount[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        loadAccounts();
    }, []);

    const loadAccounts = async () => {
        try {
            const data = await accountingService.getGLAccounts();
            setAccounts(data);
        } catch (error) {
            console.error("Failed to load accounts", error);
        } finally {
            setIsLoading(false);
        }
    };

    if (isLoading) {
        return <div className="flex justify-center p-8"><Loader2 className="h-8 w-8 animate-spin" /></div>;
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Chart of Accounts</CardTitle>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Account #</TableHead>
                            <TableHead>Name</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Normal Balance</TableHead>
                            <TableHead className="text-right">Current Balance</TableHead>
                            <TableHead>Status</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {Array.isArray(accounts) && accounts.map((account) => (
                            <TableRow key={account.id}>
                                <TableCell className="font-mono">{account.accountNumber}</TableCell>
                                <TableCell className="font-medium">{account.accountName}</TableCell>
                                <TableCell>{account.accountType}</TableCell>
                                <TableCell>{account.debitIncreases ? "Debit" : "Credit"}</TableCell>
                                <TableCell className="text-right font-mono">
                                    {formatCurrency(account.balance)}
                                </TableCell>
                                <TableCell>{account.isActive ? "Active" : "Inactive"}</TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
