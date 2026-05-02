"use client"

import { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Loader2, Plus, Pencil } from "lucide-react"
import { toast } from "sonner"
import { settingsService, Currency } from "@/services/settings-service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
    DialogFooter,
    DialogDescription,
} from "@/components/ui/dialog"
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form"
import { Checkbox } from "@/components/ui/checkbox"

const currencySchema = z.object({
    currencyName: z.string().min(2, "Name is required"),
    currencyCode: z.string().min(3, "Code must be 3 characters").max(3),
    currencySymbol: z.string().min(1, "Symbol is required"),
})

export function CurrencySettings() {
    const [currencies, setCurrencies] = useState<Currency[]>([])
    const [loading, setLoading] = useState(true)
    const [open, setOpen] = useState(false)
    const [editingCurrency, setEditingCurrency] = useState<Currency | null>(null)

    const form = useForm<z.infer<typeof currencySchema>>({
        resolver: zodResolver(currencySchema),
        defaultValues: {
            currencyName: "",
            currencyCode: "",
            currencySymbol: "",
        },
    })

    useEffect(() => {
        loadData()
    }, [])

    useEffect(() => {
        if (!open) {
            setEditingCurrency(null)
            form.reset({
                currencyName: "",
                currencyCode: "",
                currencySymbol: "",
            })
        }
    }, [open, form])

    const loadData = async () => {
        setLoading(true)
        try {
            const data = await settingsService.getCurrencies()
            // Ensure data is array
            const list = Array.isArray(data) ? data : (data ? [data] : [])
            setCurrencies(list as Currency[])
        } catch (error) {
            console.error("Failed to load currencies", error)
        } finally {
            setLoading(false)
        }
    }

    const onSubmit = async (data: z.infer<typeof currencySchema>) => {
        try {
            if (editingCurrency && editingCurrency.id) {
                await settingsService.updateCurrency({ ...data, id: editingCurrency.id })
                toast.success("Currency updated successfully")
            } else {
                await settingsService.createCurrency({ ...data, isDefault: false })
                toast.success("Currency added successfully")
            }
            setOpen(false)
            loadData()
        } catch (error) {
            toast.error(editingCurrency ? "Failed to update currency" : "Failed to add currency")
        }
    }

    const handleEdit = (currency: Currency) => {
        setEditingCurrency(currency)
        form.reset({
            currencyName: currency.currencyName,
            currencyCode: currency.currencyCode,
            currencySymbol: currency.currencySymbol,
        })
        setOpen(true)
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Currency Settings</CardTitle>
                <CardDescription>Manage your store's supported currencies.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
                <div className="flex justify-end">
                    <Button onClick={() => setOpen(true)} size="sm">
                        <Plus className="h-4 w-4 mr-2" /> Add Currency
                    </Button>
                </div>

                <div className="border rounded-md">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Code</TableHead>
                                <TableHead>Symbol</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="h-24 text-center">
                                        <Loader2 className="h-6 w-6 animate-spin mx-auto" />
                                    </TableCell>
                                </TableRow>
                            ) : currencies.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="h-24 text-center text-muted-foreground">
                                        No currencies added yet. Please add one.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                currencies.map((currency) => (
                                    <TableRow key={currency.id || currency.currencyCode}>
                                        <TableCell>{currency.currencyName}</TableCell>
                                        <TableCell>{currency.currencyCode}</TableCell>
                                        <TableCell>{currency.currencySymbol}</TableCell>
                                        <TableCell className="text-right">
                                            <Button variant="ghost" size="icon" onClick={() => handleEdit(currency)}>
                                                <Pencil className="h-4 w-4" />
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </div>

                <Dialog open={open} onOpenChange={setOpen}>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>{editingCurrency ? "Edit Currency" : "Add Currency"}</DialogTitle>
                            <DialogDescription>
                                {editingCurrency ? "Update your currency details." : "Add a new currency for your store."}
                            </DialogDescription>
                        </DialogHeader>
                        <Form {...form}>
                            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                                <FormField
                                    control={form.control}
                                    name="currencyName"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Currency Name</FormLabel>
                                            <FormControl>
                                                <Input placeholder="Ghana Cedi" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <div className="grid grid-cols-2 gap-4">
                                    <FormField
                                        control={form.control}
                                        name="currencyCode"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Code</FormLabel>
                                                <FormControl>
                                                    <Input placeholder="GHS" {...field} maxLength={3} />
                                                </FormControl>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                    <FormField
                                        control={form.control}
                                        name="currencySymbol"
                                        render={({ field }) => (
                                            <FormItem>
                                                <FormLabel>Symbol</FormLabel>
                                                <FormControl>
                                                    <Input placeholder="₵" {...field} />
                                                </FormControl>
                                                <FormMessage />
                                            </FormItem>
                                        )}
                                    />
                                </div>

                                <DialogFooter>
                                    <Button type="submit">{editingCurrency ? "Update" : "Save"}</Button>
                                </DialogFooter>
                            </form>
                        </Form>
                    </DialogContent>
                </Dialog>
            </CardContent>
        </Card>
    )
}
