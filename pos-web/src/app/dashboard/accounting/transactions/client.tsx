"use client"

import { useEffect, useState } from "react"
import { JournalEntry } from "@/types/accounting"
import { accountingService } from "@/services/accounting-service"
import { DataTable } from "@/components/ui/data-table"
import { getColumns } from "./columns"
import { TransactionDetailsModal } from "./transaction-details-modal"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"

export const TransactionsClient = () => {
    const [entries, setEntries] = useState<JournalEntry[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedEntry, setSelectedEntry] = useState<JournalEntry | null>(null)

    const loadData = async () => {
        setLoading(true)
        try {
            const data = await accountingService.getJournalEntries()
            setEntries(data)
        } catch (error) {
            console.error("Failed to load transactions", error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadData()
    }, [])

    const handleView = (entry: JournalEntry) => {
        setSelectedEntry(entry)
    }

    const columns = getColumns({
        onView: handleView
    })

    return (
        <div className="space-y-4">
            <Card>
                <CardHeader>
                    <CardTitle>All Transactions</CardTitle>
                    <CardDescription>
                        A comprehensive list of all financial transactions and journal entries recorded in the system.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {loading ? (
                        <div className="flex items-center justify-center p-8">Loading transactions...</div>
                    ) : (
                        <DataTable
                            searchKey="description"
                            columns={columns}
                            data={entries}
                        />
                    )}
                </CardContent>
            </Card>

            <TransactionDetailsModal
                open={!!selectedEntry}
                onOpenChange={(open) => !open && setSelectedEntry(null)}
                entry={selectedEntry}
            />
        </div>
    )
}
