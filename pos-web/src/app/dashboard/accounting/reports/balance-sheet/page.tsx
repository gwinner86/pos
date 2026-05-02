import { Metadata } from "next"
import BalanceSheetClient from "./client"

export const metadata: Metadata = {
    title: "Balance Sheet | POS Accounting",
    description: "View Balance Sheet Statement",
}

export default function BalanceSheetPage() {
    return (
        <div className="space-y-6">
            <BalanceSheetClient />
        </div>
    )
}
