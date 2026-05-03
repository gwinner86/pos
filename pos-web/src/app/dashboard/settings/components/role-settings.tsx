"use client"

import { useEffect, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { Shield, Plus, Lock } from "lucide-react"
import { toast } from "sonner"
import { settingsService, Role, Permission } from "@/services/settings-service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
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

const roleSchema = z.object({
    roleName: z.string().min(2, "Name is required"),
    description: z.string().optional(),
})

export function RoleSettings() {
    const [roles, setRoles] = useState<Role[]>([])
    const [loading, setLoading] = useState(true)
    const [open, setOpen] = useState(false)

    // Permission state
    const [permissionsOpen, setPermissionsOpen] = useState(false)
    const [selectedRole, setSelectedRole] = useState<Role | null>(null)
    const [allPermissions, setAllPermissions] = useState<Permission[]>([])
    const [rolePermissions, setRolePermissions] = useState<number[]>([]) // IDs
    const [loadingPerms, setLoadingPerms] = useState(false)

    const form = useForm<z.infer<typeof roleSchema>>({
        resolver: zodResolver(roleSchema),
        defaultValues: {
            roleName: "",
            description: "",
        },
    })

    useEffect(() => {
        loadData()
    }, [])

    const loadData = async () => {
        try {
            const data = await settingsService.getRoles()
            setRoles(data || [])
        } catch (error) {
            console.error("Failed to load roles", error)
        } finally {
            setLoading(false)
        }
    }

    const onSubmit = async (data: z.infer<typeof roleSchema>) => {
        try {
            await settingsService.createRole(data)
            toast.success("Role created")
            setOpen(false)
            form.reset()
            loadData()
        } catch (error) {
            toast.error("Failed to create role")
        }
    }

    const handleManagePermissions = async (role: Role) => {
        setSelectedRole(role)
        setPermissionsOpen(true)
        setLoadingPerms(true)
        try {
            // Load all permissions and role's current permissions
            const [all, existing] = await Promise.all([
                settingsService.getAllPermissions(),
                settingsService.getRolePermissions(role.roleId)
            ])
            setAllPermissions(all || [])
            setRolePermissions(existing?.map(p => p.permissionId) || [])
        } catch (error) {
            toast.error("Failed to load permissions")
        } finally {
            setLoadingPerms(false)
        }
    }

    const togglePermission = (permId: number) => {
        setRolePermissions(prev =>
            prev.includes(permId)
                ? prev.filter(id => id !== permId)
                : [...prev, permId]
        )
    }

    const savePermissions = async () => {
        if (!selectedRole) return
        try {
            await settingsService.assignPermissions(selectedRole.roleId, rolePermissions)
            toast.success("Permissions updated")
            setPermissionsOpen(false)
        } catch (error) {
            toast.error("Failed to save permissions")
        }
    }

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <div>
                    <h3 className="text-lg font-medium">Roles & Permissions</h3>
                    <p className="text-sm text-muted-foreground">Manage user roles and access levels.</p>
                </div>
                <Dialog open={open} onOpenChange={setOpen}>
                    <DialogTrigger asChild>
                        <Button size="sm"><Plus className="h-4 w-4 mr-2" /> Add Role</Button>
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Create Role</DialogTitle>
                        </DialogHeader>
                        <Form {...form}>
                            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                                <FormField
                                    control={form.control}
                                    name="roleName"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Role Name</FormLabel>
                                            <FormControl>
                                                <Input placeholder="Manager" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <FormField
                                    control={form.control}
                                    name="description"
                                    render={({ field }) => (
                                        <FormItem>
                                            <FormLabel>Description</FormLabel>
                                            <FormControl>
                                                <Input placeholder="Store manager role" {...field} />
                                            </FormControl>
                                            <FormMessage />
                                        </FormItem>
                                    )}
                                />
                                <DialogFooter>
                                    <Button type="submit">Create Role</Button>
                                </DialogFooter>
                            </form>
                        </Form>
                    </DialogContent>
                </Dialog>
            </div>

            <div className="border rounded-md bg-white">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Role Name</TableHead>
                            <TableHead>Description</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-24 text-center">Loading...</TableCell>
                            </TableRow>
                        ) : roles.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-24 text-center">No roles found.</TableCell>
                            </TableRow>
                        ) : (
                            roles.map((role) => (
                                <TableRow key={role.roleId}>
                                    <TableCell className="font-medium flex items-center gap-2">
                                        <Shield className="h-4 w-4 text-muted-foreground" />
                                        {role.roleName}
                                    </TableCell>
                                    <TableCell>{role.description || "-"}</TableCell>
                                    <TableCell className="text-right">
                                        <Button variant="outline" size="sm" onClick={() => handleManagePermissions(role)}>
                                            <Lock className="h-3 w-3 mr-1" /> Permissions
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </div>

            {/* Permission Management Dialog */}
            <Dialog open={permissionsOpen} onOpenChange={setPermissionsOpen}>
                <DialogContent className="max-w-2xl max-h-[80vh] flex flex-col">
                    <DialogHeader>
                        <DialogTitle>Manage Permissions - {selectedRole?.roleName}</DialogTitle>
                        <DialogDescription>Select permissions for this role.</DialogDescription>
                    </DialogHeader>

                    <div className="flex-1 overflow-y-auto py-4">
                        {loadingPerms ? (
                            <div className="text-center py-4">Loading permissions...</div>
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {allPermissions.map((perm) => (
                                    <div key={perm.permissionId} className="flex items-start space-x-2 border p-2 rounded hover:bg-muted/50">
                                        <Checkbox
                                            id={`perm-${perm.permissionId}`}
                                            checked={rolePermissions.includes(perm.permissionId)}
                                            onCheckedChange={() => togglePermission(perm.permissionId)}
                                        />
                                        <div className="grid gap-1.5 leading-none">
                                            <label
                                                htmlFor={`perm-${perm.permissionId}`}
                                                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                                            >
                                                {perm.name}
                                            </label>
                                            {perm.description && (
                                                <p className="text-xs text-muted-foreground">
                                                    {perm.description}
                                                </p>
                                            )}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    <DialogFooter>
                        <Button variant="outline" onClick={() => setPermissionsOpen(false)}>Cancel</Button>
                        <Button onClick={savePermissions}>Save Changes</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    )
}
