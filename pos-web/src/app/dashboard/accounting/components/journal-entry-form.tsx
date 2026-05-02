"use client";

import { useState, useMemo } from "react";
import { CreateJournalEntryRequest, GLAccount } from "@/types/accounting";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Trash2, Plus, Loader2 } from "lucide-react";
import { formatCurrency } from "@/lib/utils";

interface JournalEntryFormProps {
    accounts: GLAccount[];
    onSubmit: (data: CreateJournalEntryRequest) => Promise<void>;
    onCancel: () => void;
}

interface JournalLine {
    id: number;
    accountNumber: string;
    debit: number;
    credit: number;
}

export function JournalEntryForm({ accounts, onSubmit, onCancel }: JournalEntryFormProps) {
    const [date, setDate] = useState(new Date().toISOString().split("T")[0]);
    const [description, setDescription] = useState("");
    const [lines, setLines] = useState<JournalLine[]>([
        { id: 1, accountNumber: "", debit: 0, credit: 0 },
        { id: 2, accountNumber: "", debit: 0, credit: 0 }
    ]);
    const [submitting, setSubmitting] = useState(false);

    const totals = useMemo(() => {
        return lines.reduce(
            (acc, line) => ({
                debit: acc.debit + (line.debit || 0),
                credit: acc.credit + (line.credit || 0)
            }),
            { debit: 0, credit: 0 }
        );
    }, [lines]);

    const isBalanced = Math.abs(totals.debit - totals.credit) < 0.01 && totals.debit > 0;

    const addLine = () => {
        setLines([...lines, { id: lines.length + 1 + Math.random(), accountNumber: "", debit: 0, credit: 0 }]);
    };

    const removeLine = (id: number) => {
        setLines(lines.filter(l => l.id !== id));
    };

    const updateLine = (id: number, field: keyof JournalLine, value: any) => {
        setLines(lines.map(l => l.id === id ? { ...l, [field]: value } : l));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!isBalanced) return;

        setSubmitting(true);
        try {
            await onSubmit({
                entryDate: date,
                description,
                details: lines
                    .filter(l => l.accountNumber) // remove empty lines
                    .map(l => ({
                        accountNumber: l.accountNumber,
                        debitAmount: Number(l.debit),
                        creditAmount: Number(l.credit)
                    }))
            });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                    <Label htmlFor="date">Date</Label>
                    <Input
                        id="date"
                        type="date"
                        value={date}
                        onChange={e => setDate(e.target.value)}
                        required
                    />
                </div>
                <div className="space-y-2">
                    <Label htmlFor="desc">Description</Label>
                    <Input
                        id="desc"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        placeholder="e.g. Opening Balance Adjustment"
                        required
                    />
                </div>
            </div>

            <div className="border rounded-md p-4 bg-slate-50 dark:bg-slate-900">
                <div className="flex font-semibold text-sm mb-2 text-muted-foreground px-2">
                    <div className="flex-1">Account</div>
                    <div className="w-32 text-right">Debit</div>
                    <div className="w-32 text-right">Credit</div>
                    <div className="w-10"></div>
                </div>

                <div className="space-y-2">
                    {lines.map((line) => (
                        <div key={line.id} className="flex gap-2 items-start">
                            <div className="flex-1">
                                <Select
                                    value={line.accountNumber}
                                    onValueChange={val => updateLine(line.id, "accountNumber", val)}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Select Account" />
                                    </SelectTrigger>
                                    <SelectContent className="max-h-60">
                                        {accounts.map(acc => (
                                            <SelectItem key={acc.id} value={acc.accountNumber}>
                                                {acc.accountNumber} - {acc.accountName}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                            <div className="w-32">
                                <Input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={line.debit || ""}
                                    onChange={e => {
                                        updateLine(line.id, "debit", parseFloat(e.target.value) || 0);
                                        updateLine(line.id, "credit", 0); // Clear credit if debit is set
                                    }}
                                    className="text-right"
                                    placeholder="0.00"
                                />
                            </div>
                            <div className="w-32">
                                <Input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={line.credit || ""}
                                    onChange={e => {
                                        updateLine(line.id, "credit", parseFloat(e.target.value) || 0);
                                        updateLine(line.id, "debit", 0); // Clear debit if credit is set
                                    }}
                                    className="text-right"
                                    placeholder="0.00"
                                />
                            </div>
                            <div className="w-10 pt-2 text-center">
                                <button type="button" onClick={() => removeLine(line.id)} className="text-red-500 hover:text-red-700">
                                    <Trash2 className="h-4 w-4" />
                                </button>
                            </div>
                        </div>
                    ))}
                </div>

                <Button type="button" variant="ghost" size="sm" onClick={addLine} className="mt-2 text-blue-600">
                    <Plus className="mr-2 h-3 w-3" /> Add Line
                </Button>
            </div>

            <div className="flex justify-end items-center gap-8 p-4 bg-slate-100 dark:bg-slate-800 rounded-lg">
                <div className="text-right">
                    <div className="text-xs text-muted-foreground uppercase">Total Debits</div>
                    <div className="font-bold font-mono text-lg">{formatCurrency(totals.debit)}</div>
                </div>
                <div className="text-right">
                    <div className="text-xs text-muted-foreground uppercase">Total Credits</div>
                    <div className="font-bold font-mono text-lg">{formatCurrency(totals.credit)}</div>
                </div>
                <div className="text-right border-l pl-8">
                    <div className="text-xs text-muted-foreground uppercase">Difference</div>
                    <div className={`font-bold font-mono text-lg ${isBalanced ? 'text-green-600' : 'text-red-600'}`}>
                        {formatCurrency(Math.abs(totals.debit - totals.credit))}
                    </div>
                </div>
            </div>

            <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={onCancel} disabled={submitting}>Cancel</Button>
                <Button type="submit" disabled={!isBalanced || submitting}>
                    {submitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Post Journal Entry
                </Button>
            </div>
        </form>
    );
}
