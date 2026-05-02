"use client"

import { useState, useEffect } from "react"
import { format } from "date-fns"
import { Calendar as CalendarIcon, Loader2 } from "lucide-react"

import { cn, formatCurrency } from "@/lib/utils"
// Ensure you have a button component imported; shadcn usually has it
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import { reportService, TrialBalance } from "@/services/report-service"

export default function TrialBalanceClient() {
    const [date, setDate] = useState<Date | undefined>(new Date())
    const [data, setData] = useState<TrialBalance | null>(null)
    const [loading, setLoading] = useState(false)

    const fetchData = async () => {
        if (!date) return
        setLoading(true)
        try {
            const result = await reportService.getTrialBalance(date.toISOString())
            setData(result)
        } catch (error) {
            console.error(error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchData()
    }, [date])

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">Trial Balance</h1>
                    <p className="text-muted-foreground">
                        Sum of balances of all ledgers as of a specific date.
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                variant={"outline"}
                                className={cn(
                                    "w-[240px] justify-start text-left font-normal",
                                    !date && "text-muted-foreground"
                                )}
                            >
                                <CalendarIcon className="mr-2 h-4 w-4" />
                                {date ? format(date, "PPP") : <span>Pick a date</span>}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="end">
                            <Calendar
                                mode="single"
                                selected={date}
                                onSelect={setDate}
                                initialFocus
                            />
                        </PopoverContent>
                    </Popover>
                    <Button variant="outline" onClick={fetchData}>
                        Refresh
                    </Button>
                </div>
            </div>

            {loading ? (
                <div className="flex h-[400px] items-center justify-center">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
            ) : data ? (
                <Card>
                    <CardHeader>
                        <CardTitle>Trial Balance Report</CardTitle>
                        <CardDescription>
                            As of {format(new Date(data.asOfDate), "MMMM dd, yyyy")}
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Account Number</TableHead>
                                    <TableHead>Account Name</TableHead>
                                    <TableHead className="text-right">Debit</TableHead>
                                    <TableHead className="text-right">Credit</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {data.accounts.map((account) => (
                                    <TableRow key={account.accountNumber}>
                                        <TableCell>{account.accountNumber}</TableCell>
                                        <TableCell>{account.accountName}</TableCell>
                                        <TableCell className="text-right text-emerald-600">
                                            {account.debit !== 0 ? formatCurrency(account.debit) : "-"}
                                        </TableCell>
                                        <TableCell className="text-right text-red-600">
                                            {account.credit !== 0 ? formatCurrency(account.credit) : "-"}
                                        </TableCell>
                                    </TableRow>
                                ))}
                                {/* Totals Row */}
                                <TableRow className="bg-muted/50 font-bold">
                                    <TableCell colSpan={2} className="text-right">Total</TableCell>
                                    <TableCell className="text-right">{formatCurrency(data.totalDebit)}</TableCell>
                                    <TableCell className="text-right">{formatCurrency(data.totalCredit)}</TableCell>
                                </TableRow>
                            </TableBody>
                        </Table>

                        {/* Balance Check Alert */}
                        {data.totalDebit !== data.totalCredit ? (
                            <div className="mt-4 p-4 border border-red-200 bg-red-50 text-red-800 rounded-md">
                                <strong>Warning:</strong> Debits and Credits do not match! Review your journal entries.
                            </div>
                        ) : (
                            <div className="mt-4 p-4 border border-emerald-200 bg-emerald-50 text-emerald-800 rounded-md">
                                <strong>Status:</strong> Balanced.
                            </div>
                        )}
                    </CardContent>
                </Card>
            ) : null}
        </div>
    )
}
