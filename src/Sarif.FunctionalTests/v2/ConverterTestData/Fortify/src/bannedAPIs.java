import java.io.IOException;
import java.lang.*;
import java.security.SecureRandom;
import java.util.Arrays;
import java.util.Random;

class BannedApis {
    public static void main(String[] args) {
        class MyRandom extends Random {
            public MyRandom(long seed) {
                super(seed);
            }

            public int next(int bits) {
                return super.next(bits);
            }
        }

        Runtime.runFinalizersOnExit(true);
        System.runFinalizersOnExit(true);

        double rd = Math.random();
        System.out.println("Got " + rd + " from Math.random()");

        Runtime.runFinalizersOnExit(false);
        System.runFinalizersOnExit(false);

        Random rg0 = new Random(31415926545L);
        Random rg1 = new Random(31415926545L);
        if (rg0.nextInt() != rg1.nextInt()
            || rg0.nextInt(360) != rg1.nextInt(360)
            // || rg0.next(51) != rg1.next(51)
            || rg0.nextLong() != rg1.nextLong()
            || rg0.nextFloat() != rg1.nextFloat()
            || rg0.nextDouble() != rg1.nextDouble()
            || rg0.nextGaussian() != rg1.nextGaussian()
        ) {
            System.err.println("Unbelieveable!");
        }

        MyRandom mrg0 = new MyRandom(31415926545L);
        MyRandom mrg1 = new MyRandom(31415926545L);
        if (mrg0.nextInt() != mrg1.nextInt()
            || mrg0.nextInt(360) != mrg1.nextInt(360)
            || mrg0.next(51) != mrg1.next(51)
            || mrg0.nextLong() != mrg1.nextLong()
            || mrg0.nextFloat() != mrg1.nextFloat()
            || mrg0.nextDouble() != mrg1.nextDouble()
            || mrg0.nextGaussian() != mrg1.nextGaussian()
        ) {
            System.err.println("Unbelieveable!!");
        }

        byte[] seed = SecureRandom.getSeed(13);
        SecureRandom srg = new SecureRandom(seed);
        SecureRandom srg2 = new SecureRandom(seed);
        System.out.println("The default secure random algorithm is " + srg.getAlgorithm());
        byte[] bytes = new byte[seed.length];
        srg.nextBytes(bytes);
        srg.setSeed(seed);
        srg.nextBytes(seed);
        if (Arrays.equals(seed, bytes)) {
            System.err.println(Arrays.toString(seed));
            System.err.println(Arrays.toString(bytes));
            System.err.println("Not that secure and random!!!");
        }
        srg.setSeed(31415926545L);
        srg.nextBytes(seed);
        if (Arrays.equals(seed, bytes)) {
            System.err.println("Unbelieveable!!!");
        }
        srg2.setSeed(31415926545L);
        srg.nextBytes(bytes);
        if (Arrays.equals(seed, bytes)) {
            System.err.println("Unbelieveable2!!!");
        }

        try {
            System.out.println("There may be more coming.  See if you can hunt them all.");
            Thread.sleep(3000);
            Process p = Runtime.getRuntime().exec("java BannedApis");
            if (p != null) {
                Thread.sleep(3600);
                p.destroy();
            }
        }
        catch (IOException ex) {
            ex.printStackTrace();
        }
        catch (Throwable t) {
            t.printStackTrace();
        }
    }
}
