import { Metadata } from "next"
import IncomeStatementClient from "./client"

export const metadata: Metadata = {
    title: "Income Statement | POS Accounting",
    description: "View Profit and Loss Statement",
}

export default function IncomeStatementPage() {
    return (
        <div className="space-y-6">
            <IncomeStatementClient />
        </div>
    )
}
