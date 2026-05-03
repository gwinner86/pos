"use client"

import { Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis, CartesianGrid } from "recharts"
import { formatCurrency } from "@/lib/utils"
interface OverviewChartProps {
    data: {
        date: string;
        totalSales: number;
    }[];
}

export function OverviewChart({ data }: OverviewChartProps) {
    if (!data || data.length === 0) {
        return (
            <div className="flex items-center justify-center h-[350px] text-muted-foreground">
                No data available for chart.
            </div>
        )
    }

    return (
        <ResponsiveContainer width="100%" height={350}>
            <LineChart data={data}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis
                    dataKey="date"
                    stroke="#888888"
                    fontSize={12}
                    tickLine={false}
                    axisLine={false}
                />
                <YAxis
                    stroke="#888888"
                    fontSize={12}
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(value) => formatCurrency(value)}
                />
                <Tooltip
                    formatter={(value: any) => [formatCurrency(Number(value)), "Sales"]}
                    contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                />
                <Line
                    type="monotone"
                    dataKey="totalSales"
                    stroke="#2563eb" // Primary blue
                    strokeWidth={2}
                    activeDot={{ r: 6 }}
                    dot={false}
                />
            </LineChart>
        </ResponsiveContainer>
    )
}
