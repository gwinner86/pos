"use client";

import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip, Legend } from "recharts";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency } from "@/lib/utils";

export interface PaymentMethodStat {
    paymentMethod: string;
    transactionCount: number;
    totalAmount: number;
}

interface PaymentPieChartProps {
    data: PaymentMethodStat[];
}

const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#a855f7", "#ec4899"];

export function PaymentPieChart({ data }: PaymentPieChartProps) {
    const defaultData = data && data.length > 0 ? data.map(d => ({
        name: d.paymentMethod,
        value: d.totalAmount,
        count: d.transactionCount
    })) : [];

    return (
        <Card className="h-full">
            <CardHeader>
                <CardTitle className="text-base font-semibold">Payment Methods</CardTitle>
            </CardHeader>
            <CardContent>
                {defaultData.length > 0 ? (
                    <div className="h-[300px] w-full">
                        <ResponsiveContainer width="100%" height="100%">
                            <PieChart>
                                <Pie
                                    data={defaultData}
                                    cx="50%"
                                    cy="50%"
                                    innerRadius={60}
                                    outerRadius={80}
                                    paddingAngle={5}
                                    dataKey="value"
                                >
                                    {defaultData.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip
                                    formatter={(value: number | undefined) => formatCurrency(value || 0)}
                                    labelFormatter={() => ""}
                                />
                                <Legend verticalAlign="bottom" height={36} />
                            </PieChart>
                        </ResponsiveContainer>
                    </div>
                ) : (
                    <p className="text-sm text-muted-foreground text-center py-4">No payment data available.</p>
                )}
            </CardContent>
        </Card>
    );
}
