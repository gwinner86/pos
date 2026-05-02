import { Package, ShoppingCart, BarChart, Cloud, Shield, Users } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

const features = [
    {
        title: "Inventory Management",
        description: "Track stock levels in real-time across multiple locations with automated low-stock alerts.",
        icon: Package,
        color: "text-blue-500",
        bg: "bg-blue-500/10",
    },
    {
        title: "Point of Sale",
        description: "Process sales quickly with our lightning-fast, intuitive checkout interface designed for efficiency.",
        icon: ShoppingCart,
        color: "text-green-500",
        bg: "bg-green-500/10",
    },
    {
        title: "Analytics & Reporting",
        description: "Gain actionable insights into your business performance with detailed, real-time sales reports.",
        icon: BarChart,
        color: "text-purple-500",
        bg: "bg-purple-500/10",
    },
    {
        title: "Cloud-Based",
        description: "Access your business data securely from anywhere, anytime, on any device. Always in sync.",
        icon: Cloud,
        color: "text-sky-500",
        bg: "bg-sky-500/10",
    },
    {
        title: "Secure Payments",
        description: "Enterprise-grade security for all your transactions with end-to-end encryption standards.",
        icon: Shield,
        color: "text-rose-500",
        bg: "bg-rose-500/10",
    },
    {
        title: "Customer CRM",
        description: "Build lasting relationships with built-in customer profiles, purchase history, and loyalty tools.",
        icon: Users,
        color: "text-orange-500",
        bg: "bg-orange-500/10",
    },
]

export function Features() {
    return (
        <section id="features" className="container space-y-16 py-24 md:py-32 px-4 md:px-6 bg-slate-50 dark:bg-transparent">
            <div className="mx-auto max-w-[58rem] text-center">
                <h2 className="font-bold text-3xl leading-[1.1] sm:text-3xl md:text-5xl">
                    Everything you need to run your business
                </h2>
                <p className="mt-4 text-muted-foreground sm:text-lg max-w-2xl mx-auto">
                    Powerful features designed to help you grow, streamline operations, and boost profitability.
                </p>
            </div>
            <div className="mx-auto grid justify-center gap-6 sm:grid-cols-2 md:max-w-[64rem] md:grid-cols-3">
                {features.map((feature) => {
                    const Icon = feature.icon
                    return (
                        <Card key={feature.title} className="transition-all hover:shadow-lg border-muted/60 hover:border-primary/20">
                            <CardHeader>
                                <div className={`w-12 h-12 rounded-lg flex items-center justify-center mb-4 ${feature.bg}`}>
                                    <Icon className={`h-6 w-6 ${feature.color}`} />
                                </div>
                                <CardTitle className="text-xl">{feature.title}</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <p className="text-muted-foreground leading-relaxed">
                                    {feature.description}
                                </p>
                            </CardContent>
                        </Card>
                    )
                })}
            </div>
        </section>
    )
}
