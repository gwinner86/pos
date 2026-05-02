"use client"

import { useEffect, useState } from "react"
import { Sale } from "@/types/sales"
import { salesService } from "@/services/sales-service"
import { DataTable } from "@/components/ui/data-table"
import { getColumns } from "./columns" // Fixed import path
import { Separator } from "@/components/ui/separator"
import { toast } from "sonner"
import { SaleDetailsModal } from "@/components/sales/sale-details-modal"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"

export const SalesClient = () => {
    const [sales, setSales] = useState<Sale[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedSale, setSelectedSale] = useState<Sale | null>(null)
    const [startDate, setStartDate] = useState("")
    const [endDate, setEndDate] = useState("")

    const loadSales = async () => {
        setLoading(true)
        try {
            const data = await salesService.getSales(undefined, startDate || undefined, endDate || undefined)
            setSales(data)
        } catch (error) {
            console.error(error)
            toast.error("Failed to load sales history")
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadSales()
    }, [])

    const handleView = (sale: Sale) => {
        setSelectedSale(sale)
    }

    const columns = getColumns({
        onView: handleView
    })

    return (
        <div className="flex flex-col gap-5 w-full h-full p-4">
            <div className="flex items-center justify-between w-full">
                <div className="flex flex-col gap-1">
                    <h2 className="text-3xl font-bold tracking-tight text-foreground">
                        Sales History
                    </h2>
                    <p className="text-sm text-muted-foreground">
                        View and manage completed transactions ({sales.length})
                    </p>
                </div>
            </div>

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="text-base font-medium">Filter Sales</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="flex items-end gap-4">
                        <div className="grid gap-2">
                            <Label htmlFor="startDate">Start Date</Label>
                            <Input
                                id="startDate"
                                type="date"
                                className="w-[200px]"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                            />
                        </div>
                        <div className="grid gap-2">
                            <Label htmlFor="endDate">End Date</Label>
                            <Input
                                id="endDate"
                                type="date"
                                className="w-[200px]"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                            />
                        </div>
                        <div className="flex items-center gap-2">
                            <Button onClick={() => loadSales()}>
                                Filter
                            </Button>
                            {(startDate || endDate) && (
                                <Button
                                    variant="outline"
                                    onClick={() => {
                                        setStartDate("")
                                        setEndDate("")
                                        setLoading(true)
                                        salesService.getSales(undefined, undefined, undefined)
                                            .then(setSales)
                                            .catch(() => toast.error("Failed to reset sales"))
                                            .finally(() => setLoading(false))
                                    }}
                                >
                                    Clear
                                </Button>
                            )}
                        </div>
                    </div>
                </CardContent>
            </Card>

            {loading ? (
                <div className="flex items-center justify-center p-8">
                    <div className="text-muted-foreground">Loading sales...</div>
                </div>
            ) : (
                <div className="bg-card rounded-md border shadow-sm flex-1">
                    <DataTable
                        searchKey="saleNumber"
                        columns={columns}
                        data={sales}
                    />
                </div>
            )}

            <SaleDetailsModal
                open={!!selectedSale}
                onOpenChange={(open) => !open && setSelectedSale(null)}
                sale={selectedSale}
            />
        </div>
    )
}
