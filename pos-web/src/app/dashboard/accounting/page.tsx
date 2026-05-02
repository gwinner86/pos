"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ChartOfAccounts } from "./components/chart-of-accounts";
import { JournalEntries } from "./components/journal-entries";

export default function AccountingPage() {
    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold tracking-tight">Financials</h1>
            </div>

            <Tabs defaultValue="coa" className="w-full">
                <TabsList>
                    <TabsTrigger value="coa">Chart of Accounts</TabsTrigger>
                    <TabsTrigger value="journals">Journal Entries</TabsTrigger>
                </TabsList>
                <TabsContent value="coa" className="mt-4">
                    <ChartOfAccounts />
                </TabsContent>
                <TabsContent value="journals" className="mt-4">
                    <JournalEntries />
                </TabsContent>
            </Tabs>
        </div>
    );
}
