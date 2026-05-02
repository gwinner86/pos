import { Metadata } from "next"
import TrialBalanceClient from "./client"

export const metadata: Metadata = {
    title: "Trial Balance | POS Accounting",
    description: "View Trial Balance",
}

export default function TrialBalancePage() {
    return (
        <div className="space-y-6">
            <TrialBalanceClient />
        </div>
    )
}
