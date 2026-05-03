"use client"

import { useEffect, useState } from "react"
import { useParams, useRouter } from "next/navigation"
import { Customer } from "@/types/crm"
import { customerService } from "@/services/customer-service"
import { Sale } from "@/types/sales"
import { salesService } from "@/services/sales-service"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Separator } from "@/components/ui/separator"
import { formatCurrency } from "@/lib/utils"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { ArrowLeft } from "lucide-react"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"

import { SaleDetailsModal } from "@/components/sales/sale-details-modal"

export default function CustomerDetailsPage() {
    const params = useParams()
    const router = useRouter()
    const [customer, setCustomer] = useState<Customer | null>(null)
    const [sales, setSales] = useState<Sale[]>([])
    const [selectedSale, setSelectedSale] = useState<Sale | null>(null)
    const [loading, setLoading] = useState(true)
    const [loadingSales, setLoadingSales] = useState(true)

    useEffect(() => {
        const load = async () => {
            if (params.customerId) {
                try {
                    const customerId = params.customerId as string
                    const [customerData, salesData] = await Promise.all([
                        customerService.getCustomer(customerId),
                        salesService.getSales(customerId)
                    ])
                    setCustomer(customerData)
                    setSales(salesData)
                } catch (err) {
                    console.error(err)
                    toast.error("Failed to load customer details")
                } finally {
                    setLoading(false)
                    setLoadingSales(false)
                }
            }
        }
        load()
    }, [params.customerId])

    if (loading) return <div className="p-8">Loading...</div>
    if (!customer) return <div className="p-8">Customer not found</div>

    return (
        <div className="space-y-6 p-8 pt-6">
            <div className="flex items-center gap-4">
                <Button variant="outline" size="icon" onClick={() => router.back()}>
                    <ArrowLeft className="h-4 w-4" />
                </Button>
                <div>
                    <h2 className="text-3xl font-bold tracking-tight">{customer.firstName} {customer.lastName}</h2>
                    <p className="text-sm text-muted-foreground">{customer.customerCode}</p>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <Card className="md:col-span-1">
                    <CardHeader>
                        <CardTitle>Contact Info</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div>
                            <div className="text-sm font-medium text-muted-foreground">Email</div>
                            <div>{customer.email || "N/A"}</div>
                        </div>
                        <Separator />
                        <div>
                            <div className="text-sm font-medium text-muted-foreground">Phone</div>
                            <div>{customer.phone || "N/A"}</div>
                        </div>
                        <Separator />
                        <div>
                            <div className="text-sm font-medium text-muted-foreground">Address</div>
                            <div>{customer.addressLine1 || "N/A"}</div>
                            <div>{customer.city}</div>
                        </div>
                    </CardContent>
                </Card>

                <Card className="md:col-span-1">
                    <CardHeader>
                        <CardTitle>Overview</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div>
                            <div className="text-sm font-medium text-muted-foreground">Loyalty Points</div>
                            <div className="text-xl font-bold">{customer.loyaltyPoints}</div>
                        </div>
                        <Separator />
                        <div>
                            <div className="text-sm font-medium text-muted-foreground">Account Status</div>
                            <div className={`font-bold ${customer.isActive ? "text-green-600" : "text-red-500"}`}>
                                {customer.isActive ? "Active" : "Inactive"}
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>

            <Separator />

            <div>
                <h3 className="text-xl font-semibold mb-4">Purchase History</h3>
                <Card>
                    <CardContent className="p-0">
                        {loadingSales ? (
                            <div className="p-4 text-center text-muted-foreground">Loading history...</div>
                        ) : sales.length === 0 ? (
                            <div className="p-4 text-center text-muted-foreground">
                                No sales history found.
                            </div>
                        ) : (
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Date</TableHead>
                                        <TableHead>Sale #</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead className="text-right">Total</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {sales.map((sale) => (
                                        <TableRow
                                            key={sale.id}
                                            className="cursor-pointer hover:bg-muted/50"
                                            onClick={() => setSelectedSale(sale)}
                                        >
                                            <TableCell>
                                                {new Date(sale.saleDate).toLocaleDateString()}
                                            </TableCell>
                                            <TableCell className="font-medium">{sale.saleNumber}</TableCell>
                                            <TableCell>
                                                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                    {sale.status}
                                                </span>
                                            </TableCell>
                                            <TableCell className="text-right font-bold">
                                                {formatCurrency(sale.totalAmount)}
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        )}
                    </CardContent>
                </Card>
            </div>

            <SaleDetailsModal
                open={!!selectedSale}
                onOpenChange={(open) => !open && setSelectedSale(null)}
                sale={selectedSale}
            />
        </div>
    )
}
