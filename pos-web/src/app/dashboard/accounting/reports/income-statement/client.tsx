"use client"

import { useState, useEffect } from "react"
import { format } from "date-fns"
import { Calendar as CalendarIcon, Loader2, Download } from "lucide-react"
import { DateRange } from "react-day-picker"

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
import { reportService, IncomeStatement } from "@/services/report-service"

export default function IncomeStatementClient() {
    const [date, setDate] = useState<DateRange | undefined>({
        from: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
        to: new Date(),
    })
    const [data, setData] = useState<IncomeStatement | null>(null)
    const [loading, setLoading] = useState(false)

    const fetchData = async () => {
        if (!date?.from) return
        setLoading(true)
        try {
            const result = await reportService.getIncomeStatement(
                date.from.toISOString(),
                date.to?.toISOString()
            )
            setData(result)
        } catch (error) {
            console.error(error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchData()
    }, [date]) // Auto-fetch on date change? Or manual button? Auto is nice.

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-bold tracking-tight">Income Statement</h1>
                    <p className="text-muted-foreground">
                        Profit and Loss statement for the selected period.
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                id="date"
                                variant={"outline"}
                                className={cn(
                                    "w-[300px] justify-start text-left font-normal",
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
                                    <span>Pick a date</span>
                                )}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="end">
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
                    <Button variant="outline" onClick={fetchData}>
                        Refresh
                    </Button>
                    <Button variant="ghost" disabled>
                        <Download className="mr-2 h-4 w-4" /> Export
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
                        <CardTitle>Income Statement</CardTitle>
                        <CardDescription>
                            Period: {format(new Date(data.startDate), "MMM dd, yyyy")} - {format(new Date(data.endDate), "MMM dd, yyyy")}
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-6">

                        {/* Revenue Section */}
                        <div className="space-y-2">
                            <h3 className="font-semibold text-lg text-emerald-700">Revenue</h3>
                            <div className="border rounded-md divide-y">
                                {data.revenues.length > 0 ? (
                                    data.revenues.map((item) => (
                                        <div key={item.accountId} className="flex justify-between p-3 text-sm">
                                            <span>{item.accountNumber} - {item.accountName}</span>
                                            <span>{formatCurrency(item.amount)}</span>
                                        </div>
                                    ))
                                ) : (
                                    <div className="p-3 text-sm text-muted-foreground italic">No revenue recorded</div>
                                )}
                                <div className="flex justify-between p-3 bg-muted/50 font-bold">
                                    <span>Total Revenue</span>
                                    <span>{formatCurrency(data.totalRevenue)}</span>
                                </div>
                            </div>
                        </div>

                        {/* COGS Section */}
                        <div className="space-y-2">
                            <h3 className="font-semibold text-lg text-amber-700">Cost of Goods Sold</h3>
                            <div className="border rounded-md divide-y">
                                {data.costOfGoodsSold.length > 0 ? (
                                    data.costOfGoodsSold.map((item) => (
                                        <div key={item.accountId} className="flex justify-between p-3 text-sm">
                                            <span>{item.accountNumber} - {item.accountName}</span>
                                            <span>{formatCurrency(item.amount)}</span>
                                        </div>
                                    ))
                                ) : (
                                    <div className="p-3 text-sm text-muted-foreground italic">No COGS recorded</div>
                                )}
                                <div className="flex justify-between p-3 bg-muted/50 font-bold">
                                    <span>Total COGS</span>
                                    <span>{formatCurrency(data.totalCOGS)}</span>
                                </div>
                            </div>
                        </div>

                        {/* Gross Profit */}
                        <div className="flex justify-between items-center p-4 bg-slate-100 rounded-lg border border-slate-200">
                            <span className="text-xl font-bold text-slate-900">Gross Profit</span>
                            <span className={cn("text-xl font-bold", data.grossProfit >= 0 ? "text-emerald-600" : "text-red-600")}>
                                {formatCurrency(data.grossProfit)}
                            </span>
                        </div>

                        {/* Expenses Section */}
                        <div className="space-y-2">
                            <h3 className="font-semibold text-lg text-red-700">Operating Expenses</h3>
                            <div className="border rounded-md divide-y">
                                {data.expenses.length > 0 ? (
                                    data.expenses.map((item) => (
                                        <div key={item.accountId} className="flex justify-between p-3 text-sm">
                                            <span>{item.accountNumber} - {item.accountName}</span>
                                            <span>{formatCurrency(item.amount)}</span>
                                        </div>
                                    ))
                                ) : (
                                    <div className="p-3 text-sm text-muted-foreground italic">No expenses recorded</div>
                                )}
                                <div className="flex justify-between p-3 bg-muted/50 font-bold">
                                    <span>Total Expenses</span>
                                    <span>{formatCurrency(data.totalExpenses)}</span>
                                </div>
                            </div>
                        </div>

                        {/* Net Income */}
                        <div className="flex justify-between items-center p-6 bg-slate-900 text-white rounded-lg shadow-md">
                            <span className="text-2xl font-bold">Net Income</span>
                            <span className={cn("text-2xl font-bold", data.netIncome >= 0 ? "text-emerald-400" : "text-red-400")}>
                                {formatCurrency(data.netIncome)}
                            </span>
                        </div>

                    </CardContent>
                </Card>
            ) : null}
        </div>
    )
}
