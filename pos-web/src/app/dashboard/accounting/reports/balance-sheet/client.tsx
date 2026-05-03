"use client"

import { useState, useEffect } from "react"
import { format } from "date-fns"
import { Calendar as CalendarIcon, Loader2 } from "lucide-react"

import { cn, formatCurrency } from "@/lib/utils"
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
import { reportService, BalanceSheet } from "@/services/report-service"

export default function BalanceSheetClient() {
    const [date, setDate] = useState<Date | undefined>(new Date())
    const [data, setData] = useState<BalanceSheet | null>(null)
    const [loading, setLoading] = useState(false)

    const fetchData = async () => {
        if (!date) return
        setLoading(true)
        try {
            const result = await reportService.getBalanceSheet(date.toISOString())
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

    const AccountSection = ({ title, items, total, colorClass }: { title: string, items: any[], total: number, colorClass: string }) => (
        <div className="space-y-2">
            <h3 className={cn("font-semibold text-lg", colorClass)}>{title}</h3>
            <div className="border rounded-md divide-y">
                {items.length > 0 ? (
                    items.map((item) => (
                        <div key={item.accountId || item.accountName} className="flex justify-between p-3 text-sm">
                            <span>{item.accountNumber} - {item.accountName}</span>
                            <span>{formatCurrency(item.amount)}</span>
                        </div>
                    ))
                ) : (
                    <div className="p-3 text-sm text-muted-foreground italic">No accounts</div>
                )}
                <div className="flex justify-between p-3 bg-muted/50 font-bold">
                    <span>Total {title}</span>
                    <span>{formatCurrency(total)}</span>
                </div>
            </div>
        </div>
    )

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">Balance Sheet</h1>
                    <p className="text-muted-foreground">
                        Financial position as of a specific date.
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
                        <CardTitle>Balance Sheet</CardTitle>
                        <CardDescription>
                            As of {format(new Date(data.asOfDate), "MMMM dd, yyyy")}
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-6">

                        <AccountSection
                            title="Assets"
                            items={data.assets}
                            total={data.totalAssets}
                            colorClass="text-emerald-700"
                        />

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <AccountSection
                                title="Liabilities"
                                items={data.liabilities}
                                total={data.totalLiabilities}
                                colorClass="text-red-700"
                            />

                            <AccountSection
                                title="Equity"
                                items={data.equity}
                                total={data.totalEquity}
                                colorClass="text-blue-700"
                            />
                        </div>

                        <div className="flex justify-between items-center p-4 bg-slate-100 rounded-lg border border-slate-200 font-bold">
                            <span>Total Liabilities & Equity</span>
                            <span>{formatCurrency(data.totalLiabilities + data.totalEquity)}</span>
                        </div>
                    </CardContent>
                </Card>
            ) : null}
        </div>
    )
}
