"use client"

import { useEffect, useState } from "react"
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
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select"
import { Label } from "@/components/ui/label"
import { Loader2, Plus, Pencil, Trash2, KeyRound } from "lucide-react"
import { toast } from "sonner"
import { settingsService, UserResponse, Role } from "@/services/settings-service"
import { useAuth } from "@/hooks/use-auth"
import { z } from "zod"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import Swal from 'sweetalert2'

const userSchema = z.object({
    firstName: z.string().min(1, "First Name is required"),
    lastName: z.string().min(1, "Last Name is required"),
    email: z.string().email("Invalid email address"),
    password: z.string().optional(),
    roleId: z.string().min(1, "Role is required"),
    companyId: z.string().optional(),
})

type UserFormValues = z.infer<typeof userSchema>

export function UserSettings() {
    const { selectedLocation, selectedCompany, user } = useAuth()
    const [users, setUsers] = useState<UserResponse[]>([])
    const [roles, setRoles] = useState<Role[]>([])
    const [loading, setLoading] = useState(true)
    const [open, setOpen] = useState(false)
    const [editingUser, setEditingUser] = useState<UserResponse | null>(null)

    const form = useForm<UserFormValues>({
        resolver: zodResolver(userSchema),
        defaultValues: {
            firstName: "",
            lastName: "",
            email: "",
            roleId: ""
        },
    })

    const loadData = async () => {
        setLoading(true)
        try {
            const [usersData, rolesData] = await Promise.all([
                settingsService.getUsers(false),
                settingsService.getRoles()
            ])
            setUsers(usersData)
            setRoles(rolesData)
        } catch (error) {
            toast.error("Failed to load data")
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadData()
    }, [])

    const onSubmit = async (data: UserFormValues) => {
        try {
            const payload = {
                firstName: data.firstName,
                lastName: data.lastName,
                email: data.email,
                tenantId: user?.tenantId,
                companyId: selectedCompany?.companyId,
                roleId: parseInt(data.roleId),
            }

            if (editingUser) {
                // Update logic...
                toast.info("Update logic not implemented fully yet")
            } else {
                await settingsService.createUser(payload)
                toast.success("User created successfully")
            }
            setOpen(false)
            loadData()
        } catch (error) {
            toast.error("Failed to save user")
            console.error(error)
        }
    }

    const handleDelete = async (id: string) => {
        Swal.fire({
            title: "Are you sure?",
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, delete it!"
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    await settingsService.deleteUser(id)
                    Swal.fire({
                        title: "Deleted!",
                        text: "User has been deleted.",
                        icon: "success"
                    });
                    loadData()
                } catch (error) {
                    toast.error("Failed to delete user")
                }
            }
        });
    }

    const handleResetPassword = async (user: UserResponse) => {
        Swal.fire({
            title: "Reset Password",
            text: `Are you sure you want to reset the password for ${user.firstName}?`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, reset it"
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    // response contains the TemporaryPassword
                    const response = await settingsService.resetUserPassword(user.userId)
                    const newPassword = response.data.temporaryPassword;
                    
                    Swal.fire({
                        title: "Password Reset Successfully!",
                        html: `The new temporary password is:<br><br><b style="font-size: 20px;">${newPassword}</b><br><br>Please copy this and send it securely.`,
                        icon: "success"
                    });
                } catch (error) {
                    toast.error("Failed to reset password")
                }
            }
        });
    }

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <div>
                    <CardTitle>User Management</CardTitle>
                    <CardDescription>
                        Manage users and their access.
                    </CardDescription>
                </div>
                <Button onClick={() => { setEditingUser(null); form.reset(); setOpen(true) }}>
                    <Plus className="mr-2 h-4 w-4" /> Add User
                </Button>
            </CardHeader>
            <CardContent>
                {loading ? (
                    <div className="flex justify-center p-4"><Loader2 className="h-6 w-6 animate-spin" /></div>
                ) : (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Name</TableHead>
                                <TableHead>Email</TableHead>
                                <TableHead>Role</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {users.map((u) => (
                                <TableRow key={u.userId}>
                                    <TableCell>{u.firstName} {u.lastName}</TableCell>
                                    <TableCell>{u.email}</TableCell>
                                    <TableCell>{u.roleName}</TableCell>
                                    <TableCell><Badge variant={u.isActive ? "default" : "destructive"}>{u.isActive ? "Active" : "Inactive"}</Badge></TableCell>
                                    <TableCell className="text-right space-x-2">
                                        <Button variant="ghost" size="icon" onClick={() => handleResetPassword(u)} title="Reset Password">
                                            <KeyRound className="h-4 w-4 text-blue-500" />
                                        </Button>
                                        <Button variant="ghost" size="icon" onClick={() => handleDelete(u.userId)}>
                                            <Trash2 className="h-4 w-4 text-red-500" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                )}
            </CardContent>

            <Dialog open={open} onOpenChange={setOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{editingUser ? "Edit User" : "Add User"}</DialogTitle>
                    </DialogHeader>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                            <FormField
                                control={form.control}
                                name="firstName"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>First Name</FormLabel>
                                        <FormControl><Input {...field} /></FormControl>
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
                                        <FormControl><Input {...field} /></FormControl>
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
                                        <FormControl><Input {...field} /></FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <FormField
                                control={form.control}
                                name="roleId"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Role</FormLabel>
                                        <Select onValueChange={field.onChange} defaultValue={field.value}>
                                            <FormControl>
                                                <SelectTrigger>
                                                    <SelectValue placeholder="Select a role" />
                                                </SelectTrigger>
                                            </FormControl>
                                            <SelectContent>
                                                {roles.map((role) => (
                                                    <SelectItem key={role.roleId} value={role.roleId.toString()}>
                                                        {role.roleName}
                                                    </SelectItem>
                                                ))}
                                            </SelectContent>
                                        </Select>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <DialogFooter>
                                <Button type="submit">Save</Button>
                            </DialogFooter>
                        </form>
                    </Form>
                </DialogContent>
            </Dialog>
        </Card>
    )
}
