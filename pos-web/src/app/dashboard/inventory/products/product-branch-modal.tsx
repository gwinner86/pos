"use client"

import { useEffect, useState } from "react"
import { toast } from "sonner"
import { Loader2, GitBranch } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
    DialogFooter,
} from "@/components/ui/dialog"
import { productService } from "@/services/product-service"
import { settingsService } from "@/services/settings-service"
import { Product } from "@/types/inventory"

interface ProductBranchModalProps {
    product: Product | null
    open: boolean
    onOpenChange: (open: boolean) => void
    onSuccess?: () => void
}

export function ProductBranchModal({ product, open, onOpenChange, onSuccess }: ProductBranchModalProps) {
    const [loading, setLoading] = useState(false)
    const [saving, setSaving] = useState(false)
    const [allLocations, setAllLocations] = useState<any[]>([])
    const [assignedLocationIds, setAssignedLocationIds] = useState<Set<string>>(new Set())

    useEffect(() => {
        if (!open || !product) return

        const load = async () => {
            setLoading(true)
            try {
                const [locs, assigned] = await Promise.all([
                    settingsService.getLocations(),
                    productService.getProductLocations(product.productId),
                ])
                setAllLocations(locs || [])
                setAssignedLocationIds(new Set(assigned))
            } catch {
                toast.error("Failed to load branch data")
            } finally {
                setLoading(false)
            }
        }

        load()
    }, [open, product])

    const handleToggle = (locationId: string, checked: boolean) => {
        setAssignedLocationIds(prev => {
            const updated = new Set(prev)
            if (checked) updated.add(locationId)
            else updated.delete(locationId)
            return updated
        })
    }

    const handleSave = async () => {
        if (!product) return
        setSaving(true)
        try {
            await productService.updateProductLocations(product.productId, Array.from(assignedLocationIds))
            toast.success("Branch assignments updated successfully")
            onSuccess?.()
            onOpenChange(false)
        } catch {
            toast.error("Failed to update branch assignments")
        } finally {
            setSaving(false)
        }
    }

    if (!product) return null

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[480px]">
                <DialogHeader>
                    <div className="flex items-center gap-2">
                        <GitBranch className="h-5 w-5 text-primary" />
                        <DialogTitle>Manage Branch Visibility</DialogTitle>
                    </div>
                    <DialogDescription>
                        Choose which branches <span className="font-semibold text-foreground">{product.productName}</span> should be visible at. Only checked branches will show this product.
                    </DialogDescription>
                </DialogHeader>

                <div className="py-2">
                    {loading ? (
                        <div className="flex justify-center py-8">
                            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                        </div>
                    ) : allLocations.length === 0 ? (
                        <p className="text-sm text-muted-foreground text-center py-6">No branches found.</p>
                    ) : (
                        <div className="space-y-2 max-h-72 overflow-y-auto pr-1">
                            {allLocations.map(loc => (
                                <label
                                    key={loc.id}
                                    htmlFor={`loc-${loc.id}`}
                                    className="flex items-center gap-3 rounded-lg border px-4 py-3 cursor-pointer hover:bg-muted/50 transition-colors"
                                >
                                    <Checkbox
                                        id={`loc-${loc.id}`}
                                        checked={assignedLocationIds.has(loc.id)}
                                        onCheckedChange={(checked) => handleToggle(loc.id, !!checked)}
                                    />
                                    <div className="flex flex-col">
                                        <span className="text-sm font-medium">{loc.locationName}</span>
                                        <span className="text-xs text-muted-foreground">{loc.locationType}</span>
                                    </div>
                                </label>
                            ))}
                        </div>
                    )}
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)} disabled={saving}>
                        Cancel
                    </Button>
                    <Button onClick={handleSave} disabled={saving || loading}>
                        {saving ? <><Loader2 className="h-4 w-4 mr-2 animate-spin" />Saving...</> : "Save Changes"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}
