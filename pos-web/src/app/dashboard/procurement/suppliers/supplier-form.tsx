import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/hooks/use-auth"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { CreateSupplierRequest, Supplier } from "@/types/crm"
import { supplierService } from "@/services/supplier-service"
import { toast } from "sonner"

const supplierSchema = z.object({
    supplierName: z.string().min(2, "Name is required"),
    contactName: z.string().optional(),
    phoneNumber: z.string().min(8, "Phone number is required"),
    email: z.string().email().optional().or(z.literal("")),
    address: z.string().optional(),
    isActive: z.boolean(),
    locationId: z.string().optional().nullable(),
})

type SupplierFormValues = z.infer<typeof supplierSchema>

interface SupplierFormProps {
    initialData?: Supplier | null;
    onSuccess: () => void;
}

export function SupplierForm({ initialData, onSuccess }: SupplierFormProps) {
    const [loading, setLoading] = useState(false)
    const { selectedLocation } = useAuth() // Get current location

    const form = useForm<SupplierFormValues>({
        resolver: zodResolver(supplierSchema),
        defaultValues: {
            supplierName: initialData?.supplierName || "",
            contactName: initialData?.contactName || "",
            phoneNumber: initialData?.phone || "",
            email: initialData?.contactEmail || "",
            address: initialData?.terms || "",
            isActive: initialData?.isActive ?? true,
            locationId: initialData?.locationId || selectedLocation?.id || null,
        },
    })
    // ... rest of component


    // Reset when initialData changes
    useEffect(() => {
        if (initialData) {
            form.reset({
                supplierName: initialData.supplierName,
                contactName: initialData.contactName || "",
                phoneNumber: initialData.phone || "",
                email: initialData.contactEmail || "",
                address: initialData.terms || "",
                isActive: initialData.isActive,
                locationId: initialData.locationId,
            })
        } else {
            form.reset({
                supplierName: "",
                contactName: "",
                phoneNumber: "",
                email: "",
                address: "",
                isActive: true,
                locationId: selectedLocation?.id || null,
            })
        }
    }, [initialData, form, selectedLocation])

    const onSubmit = async (data: SupplierFormValues) => {
        setLoading(true)
        try {
            // Map form values to backend DTO expectations
            const payload = {
                supplierName: data.supplierName,
                contactName: data.contactName,
                phone: data.phoneNumber,
                contactEmail: data.email,
                terms: data.address,
                isActive: data.isActive,
                locationId: data.locationId
            }

            if (initialData) {
                await supplierService.updateSupplier(initialData.id, payload)
                toast.success("Supplier updated")
            } else {
                await supplierService.createSupplier(payload as any)
                toast.success("Supplier created")
            }
            onSuccess()
            form.reset()
        } catch (error) {
            console.error(error)
            toast.error(initialData ? "Failed to update supplier" : "Failed to create supplier")
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="supplierName"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Supplier Name</FormLabel>
                            <FormControl>
                                <Input placeholder="Acme Corp" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <FormField
                    control={form.control}
                    name="contactName"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Contact Person</FormLabel>
                            <FormControl>
                                <Input placeholder="John Doe" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="phoneNumber"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Phone</FormLabel>
                                <FormControl>
                                    <Input placeholder="050..." {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                    <FormField
                        control={form.control}
                        name="email"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Email</FormLabel>
                                <FormControl>
                                    <Input placeholder="supplier@example.com" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>
                <FormField
                    control={form.control}
                    name="address"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Details / Terms</FormLabel>
                            <FormControl>
                                <Input placeholder="Address or Terms" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />
                <div className="flex justify-end pt-4">
                    <Button type="submit" disabled={loading}>
                        {loading ? (initialData ? "Updating..." : "Creating...") : (initialData ? "Update" : "Create")}
                    </Button>
                </div>
            </form>
        </Form>
    )
}
