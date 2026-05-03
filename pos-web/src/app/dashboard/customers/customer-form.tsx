"use client"

import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Button } from "@/components/ui/button"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { CreateCustomerRequest, Customer } from "@/types/crm"
import { customerService } from "@/services/customer-service"
import { toast } from "sonner"

const customerSchema = z.object({
    firstName: z.string().min(2, "First Name is required"),
    lastName: z.string().optional(),
    email: z.string().email().optional().or(z.literal("")),
    phone: z.string().min(8, "Phone number is required"),
    addressLine1: z.string().optional(),
    city: z.string().optional(),
    customerCode: z.string().min(1, "Code is required").optional(), // We might auto-gen this or make it required
})

type CustomerFormValues = z.infer<typeof customerSchema>

interface CustomerFormProps {
    initialData?: Customer | null;
    onSuccess: () => void;
}

export function CustomerForm({ initialData, onSuccess }: CustomerFormProps) {
    const [loading, setLoading] = useState(false)

    const form = useForm<CustomerFormValues>({
        resolver: zodResolver(customerSchema),
        defaultValues: {
            firstName: initialData?.firstName || "",
            lastName: initialData?.lastName || "",
            email: initialData?.email || "",
            phone: initialData?.phone || "",
            addressLine1: initialData?.addressLine1 || "",
            city: initialData?.city || "",
            customerCode: initialData?.customerCode || `CUST-${Math.floor(Math.random() * 10000)}`,
        },
    })

    useEffect(() => {
        if (initialData) {
            form.reset({
                firstName: initialData.firstName,
                lastName: initialData.lastName || "",
                email: initialData.email || "",
                phone: initialData.phone || "",
                addressLine1: initialData.addressLine1 || "",
                city: initialData.city || "",
                customerCode: initialData.customerCode || "",
            })
        } else {
            form.reset({
                firstName: "",
                lastName: "",
                email: "",
                phone: "",
                addressLine1: "",
                city: "",
                customerCode: `CUST-${Math.floor(Math.random() * 10000)}`,
            })
        }
    }, [initialData, form])

    const onSubmit = async (data: CustomerFormValues) => {
        setLoading(true)
        try {
            if (initialData) {
                await customerService.updateCustomer(initialData.id, data)
                toast.success("Customer updated successfully")
            } else {
                await customerService.createCustomer({
                    ...data,
                    customerCode: data.customerCode || ""
                })
                toast.success("Customer created successfully")
            }
            onSuccess()
            form.reset()
        } catch (error) {
            console.error(error)
            toast.error(initialData ? "Failed to update customer" : "Failed to create customer")
        } finally {
            setLoading(false)
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="firstName"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>First Name</FormLabel>
                                <FormControl>
                                    <Input placeholder="John" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                    <FormField
                        control={form.control}
                        name="lastName"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Last Name</FormLabel>
                                <FormControl>
                                    <Input placeholder="Doe" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="phone"
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
                                <Input placeholder="john@example.com" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="addressLine1"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Address</FormLabel>
                                <FormControl>
                                    <Input placeholder="GPS or Street Address" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                    <FormField
                        control={form.control}
                        name="city"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>City</FormLabel>
                                <FormControl>
                                    <Input placeholder="Accra" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="customerCode"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Customer Code</FormLabel>
                            <FormControl>
                                <Input placeholder="CUST-001" {...field} />
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
