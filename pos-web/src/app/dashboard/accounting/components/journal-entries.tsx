"use client";

import { useEffect, useState } from "react";
import { format } from "date-fns";
import { accountingService } from "@/services/accounting-service";
import { JournalEntry, GLAccount, CreateJournalEntryRequest } from "@/types/accounting";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Loader2, Plus } from "lucide-react";
import { JournalEntryForm } from "./journal-entry-form";
import { formatCurrency } from "@/lib/utils";
import { toast } from "sonner";

export function JournalEntries() {
    const [entries, setEntries] = useState<JournalEntry[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [accounts, setAccounts] = useState<GLAccount[]>([]);
    const [open, setOpen] = useState(false);

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        setIsLoading(true);
        try {
            const [journalsData, accountsData] = await Promise.all([
                accountingService.getJournalEntries(),
                accountingService.getGLAccounts()
            ]);
            setEntries(journalsData.sort((a, b) => new Date(b.entryDate).getTime() - new Date(a.entryDate).getTime()));
            setAccounts(accountsData);
        } catch (error) {
            console.error("Failed to load journal data", error);
            // toast.error("Failed to load data");
        } finally {
            setIsLoading(false);
        }
    };

    const handleCreate = async (data: CreateJournalEntryRequest) => {
        try {
            await accountingService.createJournalEntry(data);
            toast.success("Journal Entry Posted Successfully");
            setOpen(false);
            loadData(); // Refresh
        } catch (error) {
            console.error("Failed to create journal", error);
            toast.error("Failed to post journal entry");
        }
    };

    if (isLoading) {
        return <div className="flex justify-center p-8"><Loader2 className="h-8 w-8 animate-spin" /></div>;
    }

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle>Journal Entries</CardTitle>
                <Dialog open={open} onOpenChange={setOpen}>
                    <DialogTrigger asChild>
                        <Button><Plus className="mr-2 h-4 w-4" /> New Journal Entry</Button>
                    </DialogTrigger>
                    <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                        <DialogHeader>
                            <DialogTitle>Create Journal Entry</DialogTitle>
                            <DialogDescription>
                                Record a manual journal entry. Total debits must equal total credits.
                            </DialogDescription>
                        </DialogHeader>
                        <JournalEntryForm accounts={accounts} onSubmit={handleCreate} onCancel={() => setOpen(false)} />
                    </DialogContent>
                </Dialog>
            </CardHeader>
            <CardContent>
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Date</TableHead>
                            <TableHead>Description</TableHead>
                            <TableHead>Source</TableHead>
                            <TableHead className="text-right">Amount</TableHead>
                            <TableHead>Status</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {entries.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="text-center h-24 text-muted-foreground">
                                    No journal entries found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            entries.map((entry) => (
                                <TableRow key={entry.id}>
                                    <TableCell>{format(new Date(entry.entryDate), "PPP")}</TableCell>
                                    <TableCell>
                                        <div className="font-medium">{entry.description}</div>
                                        <div className="text-xs text-muted-foreground mt-1">
                                            {/* Show quick preview of lines */}
                                            {entry.details.map((d, idx) => (
                                                <div key={idx} className="flex gap-2">
                                                    <span className="font-mono text-[10px] text-gray-500">{d.accountName || d.glAccountId}</span>
                                                    {d.debitAmount > 0 ? (
                                                        <span className="text-emerald-600">Dr {formatCurrency(d.debitAmount)}</span>
                                                    ) : (
                                                        <span className="text-red-500 ml-2">Cr {formatCurrency(d.creditAmount)}</span>
                                                    )}
                                                </div>
                                            )).slice(0, 3)}
                                            {entry.details.length > 3 && <span>...</span>}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-xs bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">
                                            {entry.sourceTable || "Manual"}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right font-medium">
                                        {formatCurrency(entry.details.reduce((sum, d) => sum + d.debitAmount, 0))}
                                    </TableCell>
                                    <TableCell>{entry.isPosted ? "Posted" : "Draft"}</TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
