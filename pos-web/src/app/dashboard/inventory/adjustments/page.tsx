import { AdjustmentForm } from "./adjustment-form"

export default function AdjustmentsPage() {
    return (
        <div className="flex flex-col gap-5 w-full max-w-4xl mx-auto">
            <div className="flex flex-col gap-1">
                <h2 className="text-3xl font-bold tracking-tight text-foreground">
                    Inventory Adjustment
                </h2>
                <p className="text-sm text-muted-foreground">
                    Manually adjust stock levels for damage, corrections, or returns.
                </p>
            </div>

            <AdjustmentForm />
        </div>
    )
}
