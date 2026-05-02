import { Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "@/components/ui/card"
import { RegisterModal } from "./register-modal"

const tiers = [
    {
        name: "Starter",
        price: "$29",
        description: "Perfect for small businesses starting out.",
        features: ["Single Register", "Basic Inventory", "Daily Sales Reports", "Email Support", "Up to 500 Products"],
        featureId: 1,
    },
    {
        name: "Pro",
        price: "$79",
        description: "For growing businesses with multiple locations.",
        features: ["Unlimited Registers", "Advanced Analytics", "Multi-location Support", "Priority 24/7 Support", "Customer Loyalty Program", "API Access"],
        featureId: 2,
        popular: true,
    },
    {
        name: "Enterprise",
        price: "Custom",
        description: "Tailored solutions for large franchises.",
        features: ["Dedicated Account Manager", "Custom Integrations", "SLA Support", "Advanced Security", "Unlimited Locations", "On-premise Options"],
        featureId: 3,
    },
]

export function Pricing() {
    return (
        <section id="pricing" className="container py-24 md:py-32 px-4 md:px-6">
            <div className="mx-auto flex max-w-[58rem] flex-col items-center justify-center gap-4 text-center">
                <h2 className="text-3xl font-bold leading-[1.1] sm:text-3xl md:text-5xl">
                    Simple, transparent pricing
                </h2>
                <p className="max-w-[85%] leading-normal text-muted-foreground sm:text-lg sm:leading-7">
                    Choose the plan that's right for your business. No hidden fees.
                </p>
            </div>
            <div className="grid grid-cols-1 gap-8 md:grid-cols-3 mt-16 max-w-6xl mx-auto">
                {tiers.map((tier) => (
                    <Card
                        key={tier.name}
                        className={`flex flex-col relative transition-all duration-200 ${tier.popular
                                ? "border-primary shadow-xl scale-105 z-10 bg-background"
                                : "border-muted hover:border-primary/50 hover:shadow-md bg-background/50"
                            }`}
                    >
                        {tier.popular && (
                            <div className="absolute -top-4 left-1/2 -translate-x-1/2 rounded-full bg-primary px-4 py-1 text-sm font-medium text-primary-foreground shadow-sm">
                                Most Popular
                            </div>
                        )}
                        <CardHeader>
                            <CardTitle className="text-2xl">{tier.name}</CardTitle>
                            <CardDescription>{tier.description}</CardDescription>
                        </CardHeader>
                        <CardContent className="flex-1">
                            <div className="text-4xl font-bold mb-6">
                                {tier.price}
                                <span className="text-lg font-normal text-muted-foreground">/mo</span>
                            </div>
                            <ul className="space-y-4">
                                {tier.features.map((feature) => (
                                    <li key={feature} className="flex items-start gap-2">
                                        <Check className="h-5 w-5 text-primary shrink-0" />
                                        <span className="text-sm">{feature}</span>
                                    </li>
                                ))}
                            </ul>
                        </CardContent>
                        <CardFooter>
                            <RegisterModal defaultPlan={tier.featureId}>
                                <Button
                                    className="w-full h-11 text-base"
                                    variant={tier.popular ? "default" : "outline"}
                                >
                                    {tier.price === "Custom" ? "Contact Sales" : "Subscribe Now"}
                                </Button>
                            </RegisterModal>
                        </CardFooter>
                    </Card>
                ))}
            </div>
        </section>
    )
}
